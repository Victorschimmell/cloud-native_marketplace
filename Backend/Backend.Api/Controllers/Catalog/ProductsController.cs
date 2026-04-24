using Backend.Api.Attributes;
using Backend.Api.Contracts.Catalog.Products;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Catalog.Products;
using Backend.Api.Mappings.Common;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<ProductResponse>> GetByIdAsync([NotEmptyGuid] Guid productId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<BrowseProductResponse>>> GetProductsAsync([FromQuery][NotEmptyGuid] Guid? categoryId, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Browse products requested for category {CategoryId}, page {Page}, page size {PageSize}.",
            categoryId,
            pageRequest.Page,
            pageRequest.PageSize);

        var result = await _productService.GetBrowseProductsAsync(categoryId, pageRequest.ToDto(), cancellationToken);

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
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
