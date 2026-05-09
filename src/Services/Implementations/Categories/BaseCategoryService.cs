using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Categories;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace E_Library.Services.Implementations.Categories;

public abstract class BaseCategoryService<TId>(AppDbContext db)
    : ICategoryService<TId>
    where TId : struct
{
    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await db.Categories.AsNoTracking().ToListAsync();
        return categories.Select(c => new CategoryResponseDto(c.Name));
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(TId id)
    {
        var category = await db.Categories.FindAsync(id);

        if (category is null) return null;
        
        return new CategoryResponseDto(category.Name);
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto dto)
    {
        var category = Category.Create(dto.Name);
        
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();
        
        return new CategoryResponseDto(category.Name);
    }

    public async Task UpdateAsync(TId id, UpdateCategoryRequestDto dto)
    {
        var existingCategory = await db.Categories.FindAsync(id);

        if (existingCategory is not null)
        {
            existingCategory.UpdateName(dto.Name);
                    
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(TId id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is not null)
        {
            await db.Categories.Where(c => c.Id == category.Id).ExecuteDeleteAsync();
        }
    }
}