using E_Library.Dtos.Categories;
using ErrorOr;

namespace E_Library.Services.Abstractions;

public interface ICategoryService
{
    Task<ErrorOr<IEnumerable<CategoryResponseDto>>> GetAllAsync();
    Task<ErrorOr<CategoryResponseDto>> GetByIdAsync(EntityId id);
    Task<ErrorOr<CategoryResponseDto>> CreateAsync(CreateCategoryRequestDto dto);
    Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateCategoryRequestDto dto);
    Task<ErrorOr<Deleted>> DeleteAsync(EntityId id);
}