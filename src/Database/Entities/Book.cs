using System.ComponentModel.DataAnnotations;

namespace E_Library.Database.Entities;

public class Book : Entity<Guid>
{
    [MaxLength(30)]
    public string Title { get; private set; } = string.Empty;
    [MaxLength(300)]
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    [Required]
    public Author? Author { get; private set; }//оскільки тип nullable ставимо required
    public Guid AuthorId { get; private set; } = Guid.CreateVersion7();
    public Guid CategoryId { get; private set; } = Guid.CreateVersion7();

    private Book() {}//ef

    private Book(Guid id, string title, string description, decimal price, Guid authorId, Guid categoryId)
    {
        Id = id;
        Title = title;
        Description = description;
        Price = price;
        AuthorId = authorId;
        CategoryId = categoryId;
    }

    public static Book Create(string title, string description, decimal price, Guid authorId, Guid categoryId)
    {
        return new Book(Guid.CreateVersion7(), title, description, price, authorId, categoryId);
    }
}