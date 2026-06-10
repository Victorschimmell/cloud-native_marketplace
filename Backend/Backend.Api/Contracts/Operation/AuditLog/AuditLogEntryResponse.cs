namespace Backend.Api.Contracts.Operation.AuditLog;

public sealed record AuditLogEntryResponse
{
    public required Guid Id { get; init; }
    public Guid? ActorUserId { get; init; }
    public string? ActorIpAddress { get; init; }
    public required AuditActionType ActionType { get; init; }
    public required string TargetEntityType { get; init; }
    public required string TargetEntityId { get; init; }
    public required AuditOutcome Outcome { get; init; }
    public required string Details { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
