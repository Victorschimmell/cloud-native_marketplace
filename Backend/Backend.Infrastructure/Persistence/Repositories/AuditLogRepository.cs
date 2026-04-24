using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Operations;
using Backend.Infrastructure.Persistence;
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

    public async Task<IReadOnlyList<AuditLog>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(a => a.TargetEntityType == entityType && a.TargetEntityId == entityId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }
}
