using ELibrary.Domain.Entities;

namespace ELibrary.Application.Common.Interfaces;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Додає сутність автора до контексту відстеження (Tracker) в пам'яті.
    /// </summary>
    /// <remarks>
    /// Метод свідомо є синхронним (void) і приймає сутність домену (Author), а не DTO:
    /// 1. EF Core під капотом лише реєструє об'єкт у локальній пам'яті, реального SQL-запиту в базу зараз не йде.
    ///    Асинхронність (Task) тут не потрібна, бо немає операцій вводу-виводу.
    /// 2. Репозиторій працює суто з валідними доменними сутностями, захищеними бізнес-правилами, 
    ///    а не з сирими структурами даних (DTO).
    /// Фіксація змін у базі даних відбудеться асинхронно через IUnitOfWork.SaveChangesAsync().
    /// </remarks>
    void Add(Author author);
 
    /// <summary>
    /// Позначає сутність автора для видалення в контексті відстеження.
    /// </summary>
    /// <remarks>
    /// Метод повертає void з тих самих причин, що й Add: операція відбувається миттєво в пам'яті сервера,
    /// змінюючи стан трекера EF Core. Справжній SQL-запит DELETE виконається тільки під час виклику Unit of Work.
    /// </remarks>
    void Remove(Author author);
}