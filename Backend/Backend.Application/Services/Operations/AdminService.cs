using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminService(IUserAccountRepository userAccountRepository, IAuditLogRepository auditLogRepository)
    {
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(auditLogRepository);

        _userAccountRepository = userAccountRepository;
        _auditLogRepository = auditLogRepository;
    }

    public Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AdminOperationResponse>.NotImplemented());
    }

    public Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AdminOperationResponse>.NotImplemented());
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

