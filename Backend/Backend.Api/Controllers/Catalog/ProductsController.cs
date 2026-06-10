using Backend.Api.Attributes;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Catalog.Products;
using Backend.Api.Mappings.Common;
using Backend.Api.Mappings.Commerce.Reviews;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;

namespace Backend.Api.Controllers.Catalog;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, IReviewService reviewService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductDetailsResponse>> GetByIdAsync(
        [NotEmptyGuid] Guid productId,
        [FromQuery][NotEmptyGuid] Guid? listingId,
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Product details requested for product {ProductId}, listing {ListingId}, currency {Currency}.", productId, listingId, currency);

        var result = await _productService.GetDetailsAsync(productId, listingId, currency, cancellationToken);

        return HandleResult(result, product => product.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<BrowseProductResponse>>> GetProductsAsync(
        [FromQuery][NotEmptyGuid] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] string? currency,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Browse products requested for category {CategoryId}, search {Search}, sort {Sort}, currency {Currency}, page {Page}, page size {PageSize}.",
            categoryId,
            search,
            sort,
            currency,
            pageRequest.Page,
            pageRequest.PageSize);

        var request = new App.BrowseProductsRequest(
            CategoryId: categoryId,
            Search: search,
            Sort: string.IsNullOrWhiteSpace(sort) ? "newest" : sort,
            Currency: string.IsNullOrWhiteSpace(currency) ? "BRL" : currency,
            Page: pageRequest.Page,
            PageSize: pageRequest.PageSize);

        var result = await _productService.GetBrowseProductsAsync(request, cancellationToken);

        return HandleResult(
            result,
            page => new PageResponse<BrowseProductResponse>
            {
                Items = page.Items.Select(product => product.ToResponse()).ToArray(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            });
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProductAsync([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPut("{productId:guid}")]
    public async Task<ActionResult<ProductResponse>> UpdateProductAsync([NotEmptyGuid] Guid productId, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        // NOTE: mapping UpdateProductRequest to UpdateProductRequest in application layer should also include the productId from route
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> DeleteProductAsync([NotEmptyGuid] Guid productId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{productId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetByProductAsync([NotEmptyGuid] Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetByProductAsync(productId, cancellationToken);
        return HandleResult(result, reviews => reviews.Select(review => review.ToResponse()).ToArray());
    }
}
