using Backend.Domain.Entities.Catalog;
using Backend.Application.Common.Models;
using Backend.Application.DTOs;

namespace Backend.Application.Abstractions.Repositories;

public interface IProductListingRepository
{
    Task<ProductListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListing>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductListing>> GetAvailableForBrowseAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default);
    Task<ProductListing?> GetAvailableProductDetailAsync(Guid productId, Guid? listingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListing>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListing>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductListing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductListing listing, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductListing listing, CancellationToken cancellationToken = default);
}
