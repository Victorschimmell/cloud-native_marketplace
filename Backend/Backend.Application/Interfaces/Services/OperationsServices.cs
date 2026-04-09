using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default);
}

public interface IAdminService
{
    Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditLogService
{
    Task<Result<PagedResult<AuditLogEntryDto>>> GetByActorUserAsync(Guid actorUserId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogEntryDto>>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<Result<AuditLogEntryDto>> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default);
}
