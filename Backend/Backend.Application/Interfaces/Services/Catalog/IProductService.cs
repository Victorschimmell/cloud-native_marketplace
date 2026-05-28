using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<ProductDetailsDto>> GetDetailsAsync(Guid productId, Guid? listingId, string? currency, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<BrowseProductDto>>> GetBrowseProductsAsync(BrowseProductsRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteListingAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SellerListingDto>>> GetSellerListingsAsync(CancellationToken cancellationToken = default);
}
