using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAdminIssueService
{
    Task<Result<AdminIssueDto>> CreateAsync(CreateAdminIssueRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminIssueDto>> ResolveAsync(ResolveAdminIssueRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminIssueDto>> AssignAsync(AssignAdminIssueRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminIssueDto>> GetByIdAsync(Guid issueId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AdminIssueDto>>> GetIssuesAsync(GetAdminIssuesRequest request, CancellationToken cancellationToken = default);
}
