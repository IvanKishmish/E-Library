using System.ComponentModel.DataAnnotations;

namespace E_Library.Database.Entities;

public class Category : Entity<Guid>
{
    [MaxLength(20)]
    public string Name { get; private set; } = string.Empty;
    
    private Category(){} //для EF core

    private Category(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(Guid.CreateVersion7(), name);
    }
}