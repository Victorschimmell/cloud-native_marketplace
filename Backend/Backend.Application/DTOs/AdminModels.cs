using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Enums;

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
    AuditActionType ActionType,
    string TargetEntityType,
    string TargetEntityId,
    AuditOutcome Outcome,
    string Details);

public sealed record GetAuditLogsRequest(Guid? ActorUserId, string? TargetEntityType, string? TargetEntityId, int Page = 1, int PageSize = 20);

public sealed record AdminBlockUserRequest(Guid UserId, string? Reason);

public sealed record AdminUnblockUserRequest(Guid UserId, string? Reason);

public sealed record AdminOperationResponse(Guid UserId, string Operation, string? Message);

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    string? Company,
    Guid? SellerId,
    Guid? PendingVerificationRequestId,
    DateTimeOffset RegisteredAtUtc,
    DateTimeOffset? LastLoginAtUtc);

public sealed record GetAdminUsersRequest(
    AdminUserRoleFilter Role,
    AdminUserStatusFilter Status,
    int Page = 1,
    int PageSize = 50);

public sealed record AdminPaymentDto(
    Guid OrderId,
    int PaymentSequential,
    string DisplayId,
    string CustomerName,
    DateTimeOffset Date,
    decimal Amount,
    string CurrencyCode,
    string Status);

public sealed record GetAdminPaymentsRequest(
    int Page = 1,
    int PageSize = 50,
    string? Currency = null,
    AdminPaymentStatusFilter Status = AdminPaymentStatusFilter.Any);

public sealed record DashboardStatsDto(
    int ActiveUsers,
    int OrdersInLast24Hours,
    decimal TotalRevenue,
    string CurrencyCode,
    int UnresolvedIssues,
    DateTimeOffset GeneratedAtUtc);

public sealed record GetDashboardStatsRequest(string? Currency = null);
