using Backend.Domain.Entities.Orders;

namespace Backend.Application.Abstractions.Repositories;

public interface IOrderReviewRepository
{
    Task<OrderReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForOrderItemAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default);
    Task AddAsync(OrderReview review, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderReview review, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderReview review, CancellationToken cancellationToken = default);
}
