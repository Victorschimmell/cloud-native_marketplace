using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await OrdersWithDetails()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .OrderByDescending(o => o.OrderPurchaseTimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Shipments);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.OrderPurchaseTimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>(orders, page, pageSize, totalCount);
    }

    public async Task<PagedResult<Order>> GetByCustomerIdWithDetailsAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = OrdersWithDetails()
            .Where(o => o.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.OrderPurchaseTimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>(orders, page, pageSize, totalCount);
    }

    public async Task<SalesAggregate> GetSalesAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking();

        if (fromUtc.HasValue)
        {
            query = query.Where(o => o.OrderPurchaseTimestampUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(o => o.OrderPurchaseTimestampUtc <= toUtc.Value);
        }

        // Revenue includes any placed order that wasn't cancelled or returned.
        query = query.Where(o => o.OrderStatus != OrderStatus.Cancelled && o.OrderStatus != OrderStatus.Returned);

        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new SalesAggregate(aggregate?.OrderCount ?? 0, aggregate?.TotalSales ?? 0m);
    }

    public async Task<OrderStatusAggregate> GetOrderStatusAggregateAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking();

        if (fromUtc.HasValue)
        {
            query = query.Where(o => o.OrderPurchaseTimestampUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(o => o.OrderPurchaseTimestampUtc <= toUtc.Value);
        }

        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Cancelled = g.Count(o => o.OrderStatus == OrderStatus.Cancelled),
                Completed = g.Count(o => o.OrderStatus == OrderStatus.Delivered)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new OrderStatusAggregate(
            aggregate?.Total ?? 0,
            aggregate?.Cancelled ?? 0,
            aggregate?.Completed ?? 0);
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Remove(order);
        return Task.CompletedTask;
    }

    private IQueryable<Order> OrdersWithDetails() =>
        dbContext.Orders
            .AsSplitQuery()
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Seller)
            .Include(o => o.Payments)
            .Include(o => o.Reviews)
                .ThenInclude(r => r.Customer)
            .Include(o => o.Shipments);
}
