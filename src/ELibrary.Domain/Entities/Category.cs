namespace ELibrary.Domain.Entities;

using ErrorOr;

public class Category : Entity<EntityId>
{
    public string Name { get; private set; } = string.Empty;
    
    private Category(){} //для EF core

    private Category(EntityId id, string name)
    :base(id)
    {
        Name = name;
    }

    public static ErrorOr<Category> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Category.NameRequired", "Name cannot be empty.");
        
        return new Category(EntityId.CreateVersion7(), name);
    }
    
    public ErrorOr<Updated> UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Error.Validation("Category.NameRequired");

        Name = newName;

        return Result.Updated;
    }
}