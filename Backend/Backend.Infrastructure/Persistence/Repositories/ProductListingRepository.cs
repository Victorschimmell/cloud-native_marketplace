using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Enums;
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

    public async Task<PagedResult<ProductListing>> GetAvailableForBrowseAsync(Guid? categoryId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ProductListings
            .AsNoTracking()
            .Include(l => l.Product)
                .ThenInclude(p => p!.Category)
            .Where(l =>
                !l.IsDeleted &&
                l.VisibilityStatus == ListingVisibilityStatus.Published &&
                l.Product != null);

        if (categoryId.HasValue)
        {
            query = query.Where(l => l.Product!.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var listings = await query
            .OrderByDescending(l => l.PublishedAtUtc ?? l.CreatedAtUtc)
            .ThenBy(l => l.Product!.ProductName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListing>(listings, request.Page, request.PageSize, totalCount);
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

    public Task AddAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        dbContext.ProductListings.Add(listing);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        dbContext.ProductListings.Update(listing);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductListing listing, CancellationToken cancellationToken = default)
    {
        listing.IsDeleted = true;
        dbContext.ProductListings.Update(listing);
        return Task.CompletedTask;
    }
}
