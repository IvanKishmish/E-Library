using ELibrary.Domain.Entities;

namespace ELibrary.Application.Common.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);
    
    void Add(Book book);
    
    void Remove(Book book);
}