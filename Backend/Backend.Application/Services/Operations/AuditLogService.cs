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
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuditLogService(IAuditLogRepository auditLogRepository, IDateTimeProvider dateTimeProvider, ICurrentUserProvider currentUserProvider)
    {
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currentUserProvider);

        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Result> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var logEntry = new AuditLog
        {
            ActorUserId = _currentUserProvider.UserId,
            ActorIpAddress = _currentUserProvider.IpAddress,
            ActionType = request.ActionType,
            TargetEntityType = request.TargetEntityType,
            TargetEntityId = request.TargetEntityId,
            Outcome = request.Outcome,
            Details = request.Details,
            CreatedAtUtc = now
        };

        await _auditLogRepository.AddAsync(logEntry, cancellationToken);

        return Result.Success();
    }
}
