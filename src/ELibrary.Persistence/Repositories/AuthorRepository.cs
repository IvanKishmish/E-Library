using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ELibrary.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Persistence.Repositories;

public sealed class AuthorRepository(AppDbContext context) : IAuthorRepository
{
    public async Task<Author?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        return await context.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Authors
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(Author author) => context.Authors.Add(author);

    public void Remove(Author author) => context.Authors.Remove(author);
}