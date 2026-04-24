using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class SellerRepository(ApplicationDbContext dbContext) : ISellerRepository
{
    public async Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Seller>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .OrderBy(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        dbContext.Sellers.Add(seller);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        dbContext.Sellers.Update(seller);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        dbContext.Sellers.Remove(seller);
        return Task.CompletedTask;
    }
}
