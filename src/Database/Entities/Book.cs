namespace E_Library.Database.Entities;

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
    {
        Id = id;
        Title = title;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        AuthorId = authorId;
    }

    public static Book Create(string title, string description, decimal price, EntityId categoryId, EntityId authorId )
    {
        return new Book(EntityId.CreateVersion7(), title, description, price, categoryId, authorId);
    }
    
    public void UpdateInfo(string title, string description, decimal price, EntityId categoryId)
    {
        Title = title;
        Description = description;
        Price = price;
        CategoryId = categoryId;
    }
}