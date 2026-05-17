using Backend.Domain.Entities.Orders;

namespace Backend.Application.Abstractions.Repositories;

public interface IOrderItemRepository
{
    Task<OrderItem?> GetByIdAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItem>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
}
