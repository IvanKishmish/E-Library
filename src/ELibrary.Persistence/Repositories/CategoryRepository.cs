using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ELibrary.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetCategoryByIdAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        //as we use this method in commands and in queries,
        //it's safer to remove the .AsNoTracking() because when either deleting or updating ef core has to track changes
        return await context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(Category category) => context.Categories.Add(category);

    public void Remove(Category category) => context.Categories.Remove(category);
}