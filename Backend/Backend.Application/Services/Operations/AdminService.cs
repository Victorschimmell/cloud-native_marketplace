using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
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

    public Task<Result<AdminOperationResponse>> BlockUserAsync(AdminBlockUserRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AdminOperationResponse>.NotImplemented());
    }

    public Task<Result<AdminOperationResponse>> UnblockUserAsync(AdminUnblockUserRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AdminOperationResponse>.NotImplemented());
    }

    public Task<Result<PagedResult<AuditLogEntryDto>>> GetAuditLogsAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<AuditLogEntryDto>>.NotImplemented());
    }
}

