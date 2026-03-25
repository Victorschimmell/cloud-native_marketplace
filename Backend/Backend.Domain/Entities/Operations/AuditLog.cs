using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Operations;

public sealed class AuditLog : Entity<Guid>
{
    public AuditLog()
    {
        Id = Guid.NewGuid();
    }

    public Guid? ActorUserId { get; set; }
    public string? ActorIpAddress { get; set; }
    public AuditActionType ActionType { get; set; }
    public required string TargetEntityType { get; set; }
    public required string TargetEntityId { get; set; }
    public AuditOutcome Outcome { get; set; }
    public required string Details { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    public IdentityAccess.UserAccount? ActorUser { get; set; }
}
