using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Catalog;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class ProductListingRepository(ApplicationDbContext dbContext) : IProductListingRepository
{
    public async Task<ProductListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductListings
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductListing>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductListings
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductListing>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductListings
            .Where(l => l.SellerId == sellerId && !l.IsDeleted)
            .OrderBy(l => l.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductListings
            .Where(l => l.ProductId == productId && !l.IsDeleted)
            .OrderBy(l => l.ListingPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        await dbContext.ProductListings.AddAsync(listing, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        dbContext.ProductListings.Update(listing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        listing.IsDeleted = true;
        dbContext.ProductListings.Update(listing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
