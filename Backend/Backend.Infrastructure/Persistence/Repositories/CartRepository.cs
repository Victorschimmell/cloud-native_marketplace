using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class CartRepository(ApplicationDbContext dbContext) : ICartRepository
{
    public async Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ShoppingCart?> GetByIdWithProductDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await CartWithProductDetails()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ShoppingCart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, cancellationToken);
    }

    public async Task<ShoppingCart?> GetActiveByUserIdWithProductDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await CartWithProductDetails()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, cancellationToken);
    }

    public async Task<ShoppingCart?> GetActiveBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == CartStatus.Active, cancellationToken);
    }

    public async Task<ShoppingCart?> GetActiveBySessionIdWithProductDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await CartWithProductDetails()
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == CartStatus.Active, cancellationToken);
    }

    public Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        dbContext.ShoppingCarts.Add(cart);
        return Task.CompletedTask;
    }

    public Task AddItemAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        dbContext.CartItems.Add(item);
        return Task.CompletedTask;
    }

    public Task UpdateItemAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        dbContext.CartItems.Update(item);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        dbContext.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        dbContext.ShoppingCarts.Update(cart);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        dbContext.ShoppingCarts.Remove(cart);
        return Task.CompletedTask;
    }

    private IQueryable<ShoppingCart> CartWithProductDetails() =>
        dbContext.ShoppingCarts
            .Include(c => c.Items)
                .ThenInclude(i => i.Listing)
                    .ThenInclude(l => l!.Product);
}
