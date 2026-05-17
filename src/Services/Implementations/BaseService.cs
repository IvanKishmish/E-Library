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
public abstract class BaseService<TEntity, TId>(AppDbContext db)
    where TEntity : Entity<TId>//кажемо що TEntity обов'язково має бути спадкоємцем класу Entity<TId>
    where TId : struct
{
    protected async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        //оскільки ми не знаєм в якій саме таблиці шукаємо, використовуємо .Set<TEntity>()
        //шукає в бд таблицю яка відповідає класу TEntity
        return await db.Set<TEntity>().AsNoTracking().ToListAsync();
    }
    
    protected async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await db.Set<TEntity>().FindAsync(id);
    }
    
    protected async Task<TEntity> AddAsync(TEntity entity)
    {
        await db.Set<TEntity>().AddAsync(entity);
        await db.SaveChangesAsync();
        return entity;
    }
    
    protected async Task UpdateAsync(TEntity entity)
    {
        db.Set<TEntity>().Update(entity);
        await db.SaveChangesAsync();
    }
    
    protected async Task DeleteAsync(TId id)
    {
        await db.Set<TEntity>()
            .Where(e => e.Id.Equals(id))
            .ExecuteDeleteAsync();
    }
}