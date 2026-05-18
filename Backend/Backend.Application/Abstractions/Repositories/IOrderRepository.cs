using Backend.Domain.Entities.Orders;
using Backend.Application.Common.Models;

namespace Backend.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetByCustomerIdWithDetailsAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SalesAggregate> GetSalesAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default);
    Task<OrderStatusAggregate> GetOrderStatusAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Order order, CancellationToken cancellationToken = default);
}

public sealed record SalesAggregate(int OrderCount, decimal TotalSalesAmount);

public sealed record OrderStatusAggregate(int TotalOrders, int CancelledOrders, int CompletedOrders);
