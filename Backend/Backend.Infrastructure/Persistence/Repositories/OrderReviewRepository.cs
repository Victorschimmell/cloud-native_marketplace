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
            .Include(r => r.Order)
                .ThenInclude(o => o!.Customer)
            .Include(r => r.OrderItem)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Include(r => r.Order)
                .ThenInclude(o => o!.Customer)
            .Include(r => r.OrderItem)
            .Where(r => r.OrderId == orderId)
            .OrderBy(r => r.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Include(r => r.Order)
                .ThenInclude(o => o!.Customer)
            .Include(r => r.OrderItem)
            .Where(review => review.OrderItem != null && review.OrderItem.ProductId == productId)
            .OrderByDescending(review => review.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForOrderItemAsync(Guid orderId, int orderItemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .AnyAsync(review => review.OrderId == orderId && review.OrderItemId == orderItemId, cancellationToken);
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
