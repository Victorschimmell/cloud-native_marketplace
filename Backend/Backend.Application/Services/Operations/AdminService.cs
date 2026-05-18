using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(
        IUserAccountRepository userAccountRepository,
        IAuditLogRepository auditLogRepository,
        IAuditLogService auditLogService,
        ICurrentUserProvider currentUserProvider,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(auditLogRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userAccountRepository = userAccountRepository;
        _auditLogRepository = auditLogRepository;
        _auditLogService = auditLogService;
        _currentUserProvider = currentUserProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<AdminOperationResponse>.Forbidden("Only admins can block users.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AdminOperationResponse>.NotFound("User not found.");
        }

        if (user.IsBlocked)
        {
            return Result<AdminOperationResponse>.Conflict("User is already blocked.");
        }

        user.IsBlocked = true;
        user.AccountStatus = AccountStatus.Suspended;

        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var details = string.IsNullOrWhiteSpace(request.Reason)
            ? $"User {user.Id} blocked."
            : $"User {user.Id} blocked. Reason: {request.Reason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Block,
            TargetEntityType: nameof(UserAccount),
            TargetEntityId: user.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: details
        ), cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(
            user.Id,
            Operation: "Block",
            Message: "User has been blocked."));
    }

    public async Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<AdminOperationResponse>.Forbidden("Only admins can unblock users.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AdminOperationResponse>.NotFound("User not found.");
        }

        if (!user.IsBlocked)
        {
            return Result<AdminOperationResponse>.Conflict("User is not currently blocked.");
        }

        user.IsBlocked = false;
        user.AccountStatus = AccountStatus.Active;
        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;

        await _userAccountRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var details = string.IsNullOrWhiteSpace(request.Reason)
            ? $"User {user.Id} unblocked."
            : $"User {user.Id} unblocked. Reason: {request.Reason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Unblock,
            TargetEntityType: nameof(UserAccount),
            TargetEntityId: user.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: details
        ), cancellationToken);

        return Result<AdminOperationResponse>.Success(new AdminOperationResponse(
            user.Id,
            Operation: "Unblock",
            Message: "User has been unblocked."));
    }

    public async Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<AuditLogEntryDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        if (request.TargetEntityId is not null && request.TargetEntityType is null)
        {
            return Result<PagedResult<AuditLogEntryDto>>.ValidationFailure("TargetEntityType must be provided when TargetEntityId is specified.");
        }

        var result = await _auditLogRepository.GetByFilterAsync(request.ActorUserId, request.TargetEntityType, request.TargetEntityId, request.Page, request.PageSize, cancellationToken);
        var auditLogs = result.Items.Select(a => a.ToAuditLogEntryDto()).ToList();
        return Result<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>(auditLogs, result.Page, result.PageSize, result.TotalCount));
    }
}
