namespace ELibrary.Domain.Entities;

using ErrorOr;

public class Author : Entity<EntityId>
{
    public string FirstName { get; private set;} = string.Empty;
    public string LastName { get; private set;} = string.Empty;
    public string Biography { get; private set;} = string.Empty;
    public ICollection<Book> Books { get; private set;} = new List<Book>();
    
    private Author(){}//ef

    private Author(EntityId id, string firstName, string lastName, string biography)
    : base(id) //з protected Entity(TId id) => Id = id; тягнемо
    {
        FirstName = firstName;
        LastName = lastName;
        Biography = biography;
        // Список книг ініціалізується автоматично
    }

    public static ErrorOr<Author> Create(string firstName, string lastName, string biography)
    {
        if (string.IsNullOrWhiteSpace(firstName)) 
            return Error.Validation("Author.FirstNameRequired", "First name is required.");
            
        if (string.IsNullOrWhiteSpace(lastName)) 
            return Error.Validation("Author.LastNameRequired", "Last name is required.");
        
        // Передаємо тільки дані автора, бо книг при створенні у нього немає
        return new Author(EntityId.CreateVersion7(), firstName, lastName, biography);
    }

    public ErrorOr<Updated> UpdateInfo(string firstName, string lastName, string biography)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return Error.Validation("Author.FirstNameRequired");
        if (string.IsNullOrWhiteSpace(lastName)) return Error.Validation("Author.LastNameRequired");
        
        FirstName = firstName;
        LastName = lastName;
        Biography = biography;

        return Result.Updated;
    }
}