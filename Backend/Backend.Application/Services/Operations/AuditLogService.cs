using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Operations;

namespace Backend.Application.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditLogService(IAuditLogRepository auditLogRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetByActorUserAsync(Guid actorUserId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (actorUserId == Guid.Empty)
            {
                return Result<PagedResult<AuditLogEntryDto>>.ValidationFailure("Actor user id is required.");
            }

            var logs = await _auditLogRepository.GetByActorUserIdAsync(actorUserId, request.Page, request.PageSize, cancellationToken);
            var items = logs.Select(static log => log.ToDto()).ToArray();
            return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(items, request.Page, request.PageSize, items.Length));
        }, "Unable to get audit logs by actor user.");
    }

    public async Task<Result<IReadOnlyList<AuditLogEntryDto>>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            {
                return Result<IReadOnlyList<AuditLogEntryDto>>.ValidationFailure("Entity type and entity id are required.");
            }

            var logs = await _auditLogRepository.GetByTargetEntityAsync(entityType.Trim(), entityId.Trim(), cancellationToken);
            return Result<IReadOnlyList<AuditLogEntryDto>>.Success(logs.Select(static log => log.ToDto()).ToArray());
        }, "Unable to get audit logs by target entity.");
    }

    public async Task<Result<AuditLogEntryDto>> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.TargetEntityType) || string.IsNullOrWhiteSpace(request.TargetEntityId) || string.IsNullOrWhiteSpace(request.Details))
            {
                return Result<AuditLogEntryDto>.ValidationFailure("Target entity and details are required.");
            }

            var log = new AuditLog
            {
                ActorUserId = request.ActorUserId,
                ActorIpAddress = request.ActorIpAddress,
                ActionType = request.ActionType,
                TargetEntityType = request.TargetEntityType.Trim(),
                TargetEntityId = request.TargetEntityId.Trim(),
                Outcome = request.Outcome,
                Details = request.Details.Trim(),
                CreatedAtUtc = _dateTimeProvider.UtcNow
            };

            await _auditLogRepository.AddAsync(log, cancellationToken);
            return Result<AuditLogEntryDto>.Success(log.ToDto());
        }, "Unable to write audit log entry.");
    }
}
