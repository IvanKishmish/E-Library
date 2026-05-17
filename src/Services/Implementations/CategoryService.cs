using System.Text.Json;
using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Categories;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Library.Services;

public class CategoryService(AppDbContext db, IDistributedCache cache) : BaseService<Category, EntityId>(db), ICategoryService
{
    private const string AllKey = "all_categories";
    private string GetKey(EntityId id) => $"category_{id}";
    private static readonly DistributedCacheEntryOptions CacheOptions = new() 
    { 
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20) 
    };
    
    public new async Task<ErrorOr<IEnumerable<CategoryResponseDto>>> GetAllAsync()
    {
        var cached = await cache.GetStringAsync(AllKey);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<CategoryResponseDto>>(cached)!;
        
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto(c.Id, c.Name))
            .ToListAsync();
        
        await cache.SetStringAsync(AllKey, JsonSerializer.Serialize(categories), CacheOptions);
        return categories;
    }

    public new async Task<ErrorOr<CategoryResponseDto>> GetByIdAsync(EntityId id)
    {
        var key = GetKey(id);
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<CategoryResponseDto>(cached)!;
        
        var category = await base.GetByIdAsync(id);
        
        if (category is null)
            return Error.NotFound(
                code: "Category.NotFound",
                description: $"Category with id '{id}' was not found.");
            
        var response = new CategoryResponseDto(category.Id, category.Name);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), CacheOptions);
        return response;
    }

    public async Task<ErrorOr<CategoryResponseDto>> CreateAsync(CreateCategoryRequestDto dto)
    {
        var category = Category.Create(dto.Name);
        await base.AddAsync(category);
        
        await cache.RemoveAsync(AllKey);
        return new CategoryResponseDto(category.Id, category.Name);
    }

    public async Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateCategoryRequestDto dto)
    {
        var category = await base.GetByIdAsync(id);
        
        if (category is null)
            return Error.NotFound(
                code: "Category.NotFound",
                description: $"Impossible to update: Category with id '{id}' was not found.");

        category.UpdateName(dto.Name);
        await base.UpdateAsync(category);

        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        return Result.Success;
    }

    public new async Task<ErrorOr<Deleted>> DeleteAsync(EntityId id)
    {
        var category = await base.GetByIdAsync(id);
        
        if (category is null)
            return Error.NotFound(
                code: "Category.NotFound",
                description: $"Category with id '{id}' was not found.");

        await base.DeleteAsync(id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Deleted;
    }
}