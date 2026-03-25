using Backend.Domain.Entities.Carts;

namespace Backend.Application.Abstractions.Repositories;

public interface ICartRepository
{
    Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetActiveBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
}
