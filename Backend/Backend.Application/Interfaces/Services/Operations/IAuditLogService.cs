using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<Result<PagedResult<AuditLogEntryDto>>> GetByActorUserAsync(Guid actorUserId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogEntryDto>>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<Result<AuditLogEntryDto>> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default);
}
