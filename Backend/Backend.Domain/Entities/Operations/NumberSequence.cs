using Backend.Domain.Base;

namespace Backend.Domain.Entities.Operations;

public sealed class NumberSequence : Entity<Guid>
{
    public NumberSequence()
    {
        Id = Guid.NewGuid();
    }

    public required string SequenceKey { get; set; }
    public long LastValue { get; set; }
}
