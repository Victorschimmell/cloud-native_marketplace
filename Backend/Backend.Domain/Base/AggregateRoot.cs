namespace Backend.Domain.Base;

public abstract class AggregateRoot<TId> : AuditableEntity<TId>
    where TId : notnull
{
}
