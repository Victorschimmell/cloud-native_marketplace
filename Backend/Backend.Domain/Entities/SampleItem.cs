using Backend.Domain.Base;

namespace Backend.Domain.Entities;

public sealed class SampleItem : AggregateRoot<Guid>
{
    public SampleItem()
    {
        Id = Guid.NewGuid();
    }

    public required string Name { get; set; }
    public string? Description { get; set; }
}
