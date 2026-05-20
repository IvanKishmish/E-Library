using E_Library.Database;
using E_Library.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Library.Services;


/// <summary>
/// 
/// </summary>
/// <param name="db"></param>
/// <typeparam name="TEntity">тип таблиці з якою ми працюємо</typeparam>
/// <typeparam name="TId">тип первинного ключа для таблиці, EntityId, int, Guid</typeparam>
public abstract class BaseService<TEntity, TId>(AppDbContext db, ILogger logger)
    where TEntity : Entity<TId>//кажемо що TEntity обов'язково має бути спадкоємцем класу Entity<TId>
    where TId : struct
{
    private readonly string _entityName = typeof(TEntity).Name;
    
    protected async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        logger.LogInformation("Fetching all records for entity type {EntityName}", _entityName);
        //оскільки ми не знаєм в якій саме таблиці шукаємо, використовуємо .Set<TEntity>()
        //шукає в бд таблицю яка відповідає класу TEntity
        return await db.Set<TEntity>().AsNoTracking().ToListAsync();
    }
    
    protected async Task<TEntity?> GetByIdAsync(TId id)
    {
        logger.LogDebug("Querying {EntityName} by ID {EntityId}", _entityName, id);
        return await db.Set<TEntity>().FindAsync(id);
    }
    
    protected async Task<TEntity> AddAsync(TEntity entity)
    {
        logger.LogInformation("Inserting new {EntityName} with ID {EntityId} into database", _entityName, entity.Id);
        
        await db.Set<TEntity>().AddAsync(entity);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Successfully persisted {EntityName} with ID {EntityId}", _entityName, entity.Id);
        
        return entity;
    }
    
    protected async Task UpdateAsync(TEntity entity)
    {
        logger.LogInformation("Updating {EntityName} with ID {EntityId} in database", _entityName, entity.Id);
        
        db.Set<TEntity>().Update(entity);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Successfully updated {EntityName} with ID {EntityId}", _entityName, entity.Id);
    }
    
    protected async Task DeleteAsync(TId id)
    {
        logger.LogInformation("Executing database deletion for {EntityName} with ID {EntityId}", _entityName, id);
        
        int affectedRows = await db.Set<TEntity>()
            .Where(e => e.Id.Equals(id))
            .ExecuteDeleteAsync();
        
        logger.LogInformation("Deleted {Count} records of {EntityName} with ID {EntityId}", affectedRows, _entityName, id);
    }
}