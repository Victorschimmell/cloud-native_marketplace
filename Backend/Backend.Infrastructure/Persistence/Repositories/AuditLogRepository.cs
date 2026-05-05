using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActorUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(a => a.ActorUserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByTargetEntityAsync(string entityType, string entityId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(a => a.TargetEntityType == entityType && a.TargetEntityId == entityId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLog>> GetByFilterAsync(Guid? userId, string? entityType, string? entityId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> query = dbContext.AuditLogs;

        if (userId.HasValue && userId != Guid.Empty)
        {
            query = query.Where(a => a.ActorUserId == userId);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.TargetEntityType == entityType);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.TargetEntityId == entityId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var auditLogs = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>(auditLogs, page, pageSize, totalCount);
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }
}
