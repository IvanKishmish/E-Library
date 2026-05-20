using System.Text.Json;
using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Categories;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Library.Services;

public class CategoryService(AppDbContext db, IDistributedCache cache, ILogger<CategoryService> logger)
    : BaseService<Category, EntityId>(db, logger), ICategoryService
{
    private const string AllKey = "all_categories";
    private string GetKey(EntityId id) => $"category_{id}";
    private static readonly DistributedCacheEntryOptions CacheOptions = new() 
    { 
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20) 
    };
    
    public new async Task<ErrorOr<IEnumerable<CategoryResponseDto>>> GetAllAsync()
    {
        logger.LogInformation("Fetching all categories from system");
        
        var cached = await cache.GetStringAsync(AllKey);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogDebug("Categories retrieved from distributed cache");
            return JsonSerializer.Deserialize<List<CategoryResponseDto>>(cached)!;
        }
        
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto(c.Id, c.Name))
            .ToListAsync();
        
        logger.LogInformation("Retrieved {Count} categories from database for caching", categories.Count);
        
        await cache.SetStringAsync(AllKey, JsonSerializer.Serialize(categories), CacheOptions);
        return categories;
    }

    public new async Task<ErrorOr<CategoryResponseDto>> GetByIdAsync(EntityId id)
    {
        logger.LogDebug("Checking cache for category with ID {CategoryId}", id);
        
        var key = GetKey(id);
        
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogDebug("Category {CategoryId} successfully found in cache", id);
            return JsonSerializer.Deserialize<CategoryResponseDto>(cached)!;
        }
        
        var category = await base.GetByIdAsync(id);

        if (category is null)
        {
            logger.LogWarning("Category business validation failed: ID {CategoryId} not found", id);
            
            return Error.NotFound(
                            code: "Category.NotFound",
                            description: $"Category with id '{id}' was not found.");
        }
        
        var response = new CategoryResponseDto(category.Id, category.Name);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), CacheOptions);
        return response;
    }

    public async Task<ErrorOr<CategoryResponseDto>> CreateAsync(CreateCategoryRequestDto dto)
    {
        try
        {
            var category = Category.Create(dto.Name);
            
            // Strictly structured logging: плейсхолдери PascalCase без символу $
            logger.LogInformation("Initiating category creation workflow for: {CategoryName}", dto.Name);
            
            await base.AddAsync(category);
            
            logger.LogInformation("Invalidating 'all categories' cache due to new category creation");
            
            await cache.RemoveAsync(AllKey);
            
            return new CategoryResponseDto(category.Id, category.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute business logic for creating category {CategoryName}", dto.Name);
            return Error.Failure("Category.CreateError", "An unexpected error occurred.");
        }
        
    }

    public async Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateCategoryRequestDto dto)
    {
        logger.LogInformation("Requested update for category {CategoryId} with new name {NewName}", id, dto.Name);
        
        var category = await base.GetByIdAsync(id);

        if (category is null)
        {
            logger.LogWarning("Update rejected: Category {CategoryId} does not exist", id);
            return Error.NotFound(
                            code: "Category.NotFound",
                            description: $"Impossible to update: Category with id '{id}' was not found.");
        }
        
        category.UpdateName(dto.Name);
        await base.UpdateAsync(category);

        logger.LogDebug("Clearing caches for updated category {CategoryId}", id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Success;
    }

    public new async Task<ErrorOr<Deleted>> DeleteAsync(EntityId id)
    {
        logger.LogInformation("Processing business request to delete category {CategoryId}", id);
        
        var category = await base.GetByIdAsync(id);

        if (category is null)
        {
            logger.LogWarning("Delete canceled: Category {CategoryId} is missing", id);
            return Error.NotFound(
                code: "Category.NotFound",
                description: $"Category with id '{id}' was not found.");
        }
        
        await base.DeleteAsync(id);
        
        logger.LogDebug("Clearing caches for deleted category {CategoryId}", id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Deleted;
    }
}