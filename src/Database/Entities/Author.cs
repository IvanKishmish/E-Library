namespace E_Library.Database.Entities;

public class Author : Entity<EntityId>
{
    public string FirstName { get; private set;} = string.Empty;
    public string LastName { get; private set;} = string.Empty;
    public string Biography { get; private set;} = string.Empty;
    public ICollection<Book> Books { get; private set;} = new List<Book>();
    
    private Author(){}//ef

    private Author(EntityId id, string firstName, string lastName, string biography)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Biography = biography;
        // Список книг ініціалізується автоматично
    }

    public static Author Create(string firstName, string lastName, string biography)
    {
        // Передаємо тільки дані автора, бо книг при створенні у нього немає
        return new Author(EntityId.CreateVersion7(), firstName, lastName, biography);
    }

    public void UpdateInfo(string firstName, string lastName, string biography)
    {
        FirstName = firstName;
        LastName = lastName;
        Biography = biography;
    }
}