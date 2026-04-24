using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class OrderItemRepository(ApplicationDbContext dbContext) : IOrderItemRepository
{
    public async Task<OrderItem?> GetByIdAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderItems
            .FirstOrDefaultAsync(i => i.OrderId == orderId && i.OrderItemId == orderItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderItems
            .Where(i => i.OrderId == orderId)
            .OrderBy(i => i.OrderItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderItem>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderItems
            .Where(i => i.SellerId == sellerId)
            .OrderBy(i => i.OrderId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        dbContext.OrderItems.Add(orderItem);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        dbContext.OrderItems.Update(orderItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        dbContext.OrderItems.Remove(orderItem);
        return Task.CompletedTask;
    }
}
