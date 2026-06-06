using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class OrderReviewRepository(ApplicationDbContext dbContext) : IOrderReviewRepository
{
    public async Task<OrderReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var productIdsQuery = dbContext.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .Select(oi => oi.ProductId);
        
        var customerId = await dbContext.Orders
            .Where(o => o.Id == orderId)
            .Select(o => o.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        return await dbContext.OrderReviews
            .Include(r => r.Customer)
            .Where(r => productIdsQuery.Contains(r.ProductId) && r.CustomerId == customerId)
            .OrderBy(r => r.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Include(r => r.Customer)
            .Where(review => review.ProductId == productId)
            .OrderByDescending(review => review.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForOrderItemAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .AnyAsync(review => review.OrderId == orderId && review.OrderItemId == orderItemId, cancellationToken);
    }

    public async Task<bool> ExistsForCustomerProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .AnyAsync(review => review.CustomerId == customerId && review.ProductId == productId, cancellationToken);
    }

    public Task AddAsync(OrderReview review, CancellationToken cancellationToken = default)
    {
        dbContext.OrderReviews.Add(review);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(OrderReview review, CancellationToken cancellationToken = default)
    {
        dbContext.OrderReviews.Update(review);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderReview review, CancellationToken cancellationToken = default)
    {
        dbContext.OrderReviews.Remove(review);
        return Task.CompletedTask;
    }
}
