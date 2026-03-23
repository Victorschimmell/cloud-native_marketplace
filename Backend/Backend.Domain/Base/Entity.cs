namespace Backend.Domain.Base;

public abstract class Entity<TId> : BaseEntity
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
