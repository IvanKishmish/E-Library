using E_Library.Database.Entities;
using E_Library.Dtos.Categories;

namespace E_Library.Services.Abstractions;

public interface ICategoryService<TId> 
    where TId : struct
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(TId id);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto dto);
    Task UpdateAsync(TId id, UpdateCategoryRequestDto dto);
    Task DeleteAsync(TId id);
}