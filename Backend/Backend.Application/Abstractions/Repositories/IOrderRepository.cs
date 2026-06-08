using Backend.Application.Common.Models;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, OrderStatus? status = null, CustomerOrderSort sort = CustomerOrderSort.Newest, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetByCustomerIdWithDetailsAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, OrderStatus? status = null, SellerOrderSort sort = SellerOrderSort.Newest, CancellationToken cancellationToken = default);
    Task<SellerOrderAggregate> GetSellerOrderAggregateAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<SalesAggregate> GetSalesAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default);
    Task<OrderStatusAggregate> GetOrderStatusAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Order order, CancellationToken cancellationToken = default);
}

public sealed record SalesAggregate(int OrderCount, decimal TotalSalesAmount);

public sealed record OrderStatusAggregate(int TotalOrders, int CancelledOrders, int CompletedOrders);

public sealed record SellerOrderAggregate(int TotalOrders, int ActiveOrders, decimal TotalRevenue);

public enum CustomerOrderSort
{
    Newest = 1,
    Oldest = 2,
    TotalHigh = 3,
    TotalLow = 4
}

public enum SellerOrderSort
{
    Newest = 1,
    Oldest = 2,
    TotalHigh = 3,
    TotalLow = 4
}
