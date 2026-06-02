namespace ELibrary.Domain.Entities;

using ErrorOr;

public class Book : Entity<EntityId>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    
    public EntityId CategoryId { get; private set; }
    public EntityId AuthorId { get; private set; }

    
    public Category Category { get; private set; } = null!;
    public Author Author { get; private set; } = null!;

    private Book() {}//ef

    private Book(EntityId id, string title, string description, decimal price, EntityId categoryId, EntityId authorId )
    : base(id)
    {
        Title = title;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        AuthorId = authorId;
    }

    public static ErrorOr<Book> Create(string title, string description, decimal price, EntityId categoryId, EntityId authorId )
    {
        if (string.IsNullOrWhiteSpace(title)) 
            return Error.Validation("Book.TitleRequired", "Title cannot be empty.");
            
        if (price < 0) 
            return Error.Validation("Book.InvalidPrice", "Price cannot be less than zero.");
        
        return new Book(EntityId.CreateVersion7(), title, description, price, categoryId, authorId);
    }
    
    public ErrorOr<Updated> UpdateInfo(string title, string description, decimal price, EntityId categoryId)
    {
        if (string.IsNullOrWhiteSpace(title)) return Error.Validation("Book.TitleRequired");
        if (price < 0) return Error.Validation("Book.InvalidPrice");
        
        Title = title;
        Description = description;
        Price = price;
        CategoryId = categoryId;

        return Result.Updated;
    }
}