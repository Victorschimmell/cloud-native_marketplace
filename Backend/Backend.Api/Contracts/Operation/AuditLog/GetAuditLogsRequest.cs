using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Operation.AuditLog;

public sealed record GetAuditLogsRequest
{
    [NotEmptyGuid]
    public Guid? ActorUserId { get; init; }
    public string? TargetEntityType { get; init; }
    public string? TargetEntityId { get; init; }
}
