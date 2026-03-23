using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Operations;

public sealed class SystemIncident : Entity<Guid>
{
    public SystemIncident()
    {
        Id = Guid.NewGuid();
    }

    public IncidentType IncidentType { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public required string ComponentName { get; set; }
    public required string Message { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
}
