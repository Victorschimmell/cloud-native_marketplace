using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Operation.AuditLog;

public sealed record WriteAuditLogEntryRequest
{
    [NotEmptyGuid]
    public Guid? ActorUserId { get; init; }
    public string? ActorIpAddress { get; init; }
    public required AuditActionType ActionType { get; init; }
    public required string TargetEntityType { get; init; }
    public required string TargetEntityId { get; init; }
    public required AuditOutcome Outcome { get; init; }
    public required string Details { get; init; }
}
