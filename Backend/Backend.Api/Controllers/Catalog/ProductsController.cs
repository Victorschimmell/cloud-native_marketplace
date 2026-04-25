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

    public ProductsController(IProductService productService, IReviewService reviewService)
    {
        _productService = productService;
        _reviewService = reviewService;
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ProductResponse>> GetByIdAsync([NotEmptyGuid] Guid productId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<ProductModel>>> GetProductsAsync([FromQuery][NotEmptyGuid] Guid? categoryId, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        if (categoryId.HasValue)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
        }

        // All products
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
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
