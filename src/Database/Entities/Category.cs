namespace E_Library.Database.Entities;

public class Category : Entity<EntityId>
{
    public string Name { get; private set; } = string.Empty;
    
    private Category(){} //для EF core

    private Category(EntityId id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(EntityId.CreateVersion7(), name);
    }
    
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Назва не може бути порожньою");

        Name = newName;
    }
}