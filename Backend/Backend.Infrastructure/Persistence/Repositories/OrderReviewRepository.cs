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
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Where(r => r.OrderId == orderId)
            .OrderBy(r => r.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderReviews
            .Where(review => review.Order != null && review.Order.Items.Any(item => item.ProductId == productId))
            .OrderBy(review => review.ReviewCreationDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderReview review, CancellationToken cancellationToken = default)
    {
        await dbContext.OrderReviews.AddAsync(review, cancellationToken);
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
