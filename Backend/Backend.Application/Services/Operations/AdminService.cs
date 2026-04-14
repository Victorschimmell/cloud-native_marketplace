using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminService(IUserAccountRepository userAccountRepository, IAuditLogRepository auditLogRepository )
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(auditLogRepository);

        _userAccountRepository = userAccountRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.UserId == Guid.Empty)
            {
                return Result<AdminOperationResponse>.ValidationFailure("User id is required.");
            }

            var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<AdminOperationResponse>.NotFound("User was not found.");
            }

            user.IsBlocked = true;
            user.AccountStatus = AccountStatus.Suspended;
            await _userAccountRepository.UpdateAsync(user, cancellationToken);

            return Result<AdminOperationResponse>.Success(new AdminOperationResponse(user.Id, AuditActionType.Block.ToString(), request.Reason));
        }, "Unable to block user.");
    }

    public async Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.UserId == Guid.Empty)
            {
                return Result<AdminOperationResponse>.ValidationFailure("User id is required.");
            }

            var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<AdminOperationResponse>.NotFound("User was not found.");
            }

            user.IsBlocked = false;
            user.AccountStatus = AccountStatus.Active;
            await _userAccountRepository.UpdateAsync(user, cancellationToken);

            return Result<AdminOperationResponse>.Success(new AdminOperationResponse(user.Id, AuditActionType.Unblock.ToString(), request.Reason));
        }, "Unable to unblock user.");
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLog> logs;

        if (request.ActorUserId.HasValue && request.ActorUserId.Value != Guid.Empty)
        {
            logs = await _auditLogRepository.GetByActorUserIdAsync(request.ActorUserId.Value, request.Page, request.PageSize, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.TargetEntityType) && !string.IsNullOrWhiteSpace(request.TargetEntityId))
        {
            logs = await _auditLogRepository.GetByTargetEntityAsync(request.TargetEntityType.Trim(), request.TargetEntityId.Trim(), cancellationToken);
        }
        else
        {
            logs = await _auditLogRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        }

        var items = logs.Select(static log => log.ToDto()).ToArray();
        return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(items, request.Page, request.PageSize, items.Length));
    }
}

