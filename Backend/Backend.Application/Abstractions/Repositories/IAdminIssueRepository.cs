using Backend.Application.Common.Models;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;

namespace Backend.Application.Abstractions.Repositories;

public interface IAdminIssueRepository
{
    Task<AdminIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminIssue>> GetByFilterAsync(IssueStatus? status, IssuePriority? priority, bool unresolvedOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(AdminIssue issue, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdminIssue issue, CancellationToken cancellationToken = default);
}
