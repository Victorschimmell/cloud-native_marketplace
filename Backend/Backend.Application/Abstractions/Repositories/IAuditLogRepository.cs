using Backend.Domain.Entities.Operations;

namespace Backend.Application.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByActorUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByTargetEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
