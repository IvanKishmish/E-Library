using ELibrary.Application.Common.Interfaces;
using ELibrary.Persistence.Context;

namespace ELibrary.Persistence.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}