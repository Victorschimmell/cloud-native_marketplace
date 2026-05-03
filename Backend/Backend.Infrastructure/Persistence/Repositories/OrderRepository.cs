using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderPurchaseTimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderPurchaseTimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
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
}
