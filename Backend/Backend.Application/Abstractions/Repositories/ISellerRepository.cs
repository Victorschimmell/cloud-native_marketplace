using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Abstractions.Repositories;

public interface ISellerRepository
{
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Seller>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Seller seller, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seller seller, CancellationToken cancellationToken = default);
    Task DeleteAsync(Seller seller, CancellationToken cancellationToken = default);
}
