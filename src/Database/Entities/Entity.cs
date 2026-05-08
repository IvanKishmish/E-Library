namespace E_Library.Database.Entities;

public abstract class Entity<TId> : IAuditable
    where TId : notnull
{
    public TId Id { get; protected init; } = default!;
    
    public DateTimeOffset CreatedAt { get; protected init;}//тільки один раз при створюванні
    public DateTimeOffset? UpdatedAt { get; protected set;}

    protected Entity()
    {
    }

    protected Entity(TId id) => Id = id;
}