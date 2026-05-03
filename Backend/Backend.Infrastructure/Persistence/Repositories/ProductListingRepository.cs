using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.DTOs;
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
            .Include(l => l.Product)
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

    public async Task<PagedResult<ProductListing>> GetAvailableForBrowseAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ProductListings
            .AsNoTracking()
            .Include(l => l.Product)
                .ThenInclude(p => p!.Category)
            .Where(l =>
                !l.IsDeleted &&
                l.VisibilityStatus == ListingVisibilityStatus.Published &&
                l.Product != null);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(l => l.Product!.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(l =>
                EF.Functions.ILike(l.Product!.ProductName, $"%{search}%") ||
                EF.Functions.ILike(l.Product.Description, $"%{search}%") ||
                (l.Product.Category != null && (
                    EF.Functions.ILike(l.Product.Category.CategoryNamePt, $"%{search}%") ||
                    (l.Product.Category.CategoryNameEn != null && EF.Functions.ILike(l.Product.Category.CategoryNameEn, $"%{search}%")))));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orderedQuery = request.Sort switch
        {
            "price-asc" => query.OrderBy(l => l.ListingPrice).ThenBy(l => l.Product!.ProductName),
            "price-desc" => query.OrderByDescending(l => l.ListingPrice).ThenBy(l => l.Product!.ProductName),
            "name-asc" => query.OrderBy(l => l.Product!.ProductName).ThenBy(l => l.ListingPrice),
            _ => query.OrderByDescending(l => l.PublishedAtUtc ?? l.CreatedAtUtc).ThenBy(l => l.Product!.ProductName)
        };

        var listings = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListing>(listings, request.Page, request.PageSize, totalCount);
    }

    public async Task<ProductListing?> GetAvailableProductDetailAsync(Guid productId, Guid? listingId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ProductListings
            .AsNoTracking()
            .Include(l => l.Product)
                .ThenInclude(p => p!.Category)
            .Include(l => l.Seller)
            .Where(l =>
                l.ProductId == productId &&
                !l.IsDeleted &&
                l.VisibilityStatus == ListingVisibilityStatus.Published &&
                l.Product != null &&
                l.Seller != null);

        if (listingId.HasValue)
        {
            query = query.Where(l => l.Id == listingId.Value);
        }

        return await query
            .OrderBy(l => l.ListingPrice)
            .FirstOrDefaultAsync(cancellationToken);
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
