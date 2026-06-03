using ELibrary.Domain.Entities;

namespace ELibrary.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryByIdAsync(EntityId id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    
    void Add(Category category);
    
    void Remove(Category category);
}