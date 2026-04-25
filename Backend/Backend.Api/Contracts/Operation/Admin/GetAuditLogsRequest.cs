using Backend.Api.Attributes;

namespace Backend.Api.Contracts.Operation.Admin;

public sealed record GetAuditLogsRequest
{
    [NotEmptyGuid]
    public required Guid ActorUserId { get; init; }
    public string? TargetEntityType { get; init; }
    public string? TargetEntityId { get; init; }
}
