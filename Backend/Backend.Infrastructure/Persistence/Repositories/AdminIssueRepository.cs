using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class AdminIssueRepository(ApplicationDbContext dbContext) : IAdminIssueRepository
{
    public async Task<AdminIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdminIssues
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<PagedResult<AdminIssue>> GetByFilterAsync(IssueStatus? status, IssuePriority? priority, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AdminIssues.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(i => i.Priority == priority.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminIssue>(items, page, pageSize, totalCount);
    }

    public async Task<int> CountByStatusAsync(IssueStatus status, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdminIssues.CountAsync(i => i.Status == status, cancellationToken);
    }

    public Task AddAsync(AdminIssue issue, CancellationToken cancellationToken = default)
    {
        dbContext.AdminIssues.Add(issue);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AdminIssue issue, CancellationToken cancellationToken = default)
    {
        dbContext.AdminIssues.Update(issue);
        return Task.CompletedTask;
    }
}
