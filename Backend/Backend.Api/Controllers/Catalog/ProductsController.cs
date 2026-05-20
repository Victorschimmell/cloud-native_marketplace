using Backend.Api.Attributes;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Catalog.Products;
using Backend.Api.Mappings.Common;
using Backend.Api.Mappings.Commerce.Reviews;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Enums;

namespace Backend.Api.Controllers.Catalog;

[Route("api/products")]
public sealed class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IAuditLogService _auditLogService;

    public ProductsController(
        IProductService productService,
        IReviewService reviewService,
        ILogger<ProductsController> logger,
        IAuditLogService auditLogService)
    {
        _productService = productService;
        _reviewService = reviewService;
        _logger = logger;
        _auditLogService = auditLogService;
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

    [HttpGet("my-listings")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<SellerListingResponse>>> GetMyListingsAsync(CancellationToken cancellationToken)
    {
        var result = await _productService.GetSellerListingsAsync(cancellationToken);
        return HandleResult(result, listings => listings.Select(l => l.ToResponse()).ToArray());
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductResponse>> CreateProductAsync([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request.ToDto(), cancellationToken);

        if (result.IsSuccess)
        {
            await _auditLogService.WriteEntryAsync(new App.WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(Product),
                TargetEntityId: result.Value!.Id.ToString(),
                Outcome: AuditOutcome.Succeeded,
                Details: "Product created"
            ), cancellationToken);
        }

        return HandleResult(result, product => product.ToResponse());
    }

    [HttpPut("listings/{listingId:guid}")]
    [Authorize]
    public async Task<ActionResult<ProductResponse>> UpdateProductAsync([NotEmptyGuid] Guid listingId, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(request.ToDto(listingId), cancellationToken);
        return HandleResult(result, product => product.ToResponse());
    }

    [HttpDelete("listings/{listingId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductAsync([NotEmptyGuid] Guid listingId, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteListingAsync(listingId, cancellationToken);

        if (result.IsSuccess)
        {
            await _auditLogService.WriteEntryAsync(new App.WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Deleted,
                TargetEntityType: nameof(ProductListing),
                TargetEntityId: listingId.ToString(),
                Outcome: AuditOutcome.Succeeded,
                Details: "Listing deleted"
            ), cancellationToken);
        }

        return HandleResult(result);
    }

    [HttpGet("{productId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetByProductAsync([NotEmptyGuid] Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetByProductAsync(productId, cancellationToken);
        return HandleResult(result, reviews => reviews.Select(review => review.ToResponse()).ToArray());
    }
}
