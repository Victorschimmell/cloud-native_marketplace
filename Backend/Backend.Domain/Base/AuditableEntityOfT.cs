namespace Backend.Domain.Base;

public abstract class AuditableEntity<TId> : AuditableEntity
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
