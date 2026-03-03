using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public sealed class SampleItem : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
