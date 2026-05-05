using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Operation.AuditLog;

public sealed record GetAuditLogsRequest
{
    [NotEmptyGuid]
    public Guid? ActorUserId { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
}
