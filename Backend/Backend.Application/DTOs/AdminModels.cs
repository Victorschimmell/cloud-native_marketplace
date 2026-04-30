using Backend.Application.Enums;

namespace Backend.Application.DTOs;

public sealed record AuditLogEntryDto(
    Guid Id,
    Guid? ActorUserId,
    string? ActorIpAddress,
    AuditActionType ActionType,
    string TargetEntityType,
    string TargetEntityId,
    AuditOutcome Outcome,
    string Details,
    DateTimeOffset CreatedAtUtc);

public sealed record WriteAuditLogEntryRequest(
    Guid? ActorUserId,
    string? ActorIpAddress,
    AuditActionType ActionType,
    string TargetEntityType,
    string TargetEntityId,
    AuditOutcome Outcome,
    string Details);

public sealed record GetAuditLogsRequest(Guid? ActorUserId, string? TargetEntityType, string? TargetEntityId, int Page = 1, int PageSize = 20);

public sealed record AdminBlockUserRequest(Guid UserId, string? Reason);

public sealed record AdminUnblockUserRequest(Guid UserId, string? Reason);

public sealed record AdminOperationResponse(Guid UserId, string Operation, string? Message);
