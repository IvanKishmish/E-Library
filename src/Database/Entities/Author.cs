namespace E_Library.Database.Entities;

public class Author : Entity<Guid>
{
    public string FirstName { get; private set;} = string.Empty;
    public string LastName { get; private set;} = string.Empty;
    public string Biography { get; private set;} = string.Empty;
    public List<Book> Books { get; private set;} = [];
    
    private Author(){}//ef

    private Author(Guid id, string firstName, string lastName, string biography, List<Book> books)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Biography = biography;
        Books = books;
    }

    public static Author Create(Guid id, string firstName, string lastName, string biography, List<Book> books)
    {
        return new Author(Guid.CreateVersion7(), firstName, lastName, biography, books);
    }
}