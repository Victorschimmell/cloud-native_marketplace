using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Commerce.Shipments;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/orders")]
[Authorize]
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IReviewService _reviewService;
    private readonly IShipmentService _shipmentService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public OrdersController(
        IOrderService orderService,
        IReviewService reviewService,
        IShipmentService shipmentService,
        ICurrentUserProvider currentUserProvider)
    {
        _orderService = orderService;
        _reviewService = reviewService;
        _shipmentService = shipmentService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderModel>> GetByIdAsync([NotEmptyGuid] Guid orderId, [FromQuery] string? currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _orderService.GetByIdForCustomerAsync(orderId, userId, currency, cancellationToken);
        return HandleResult(result, order => order.ToModel());
    }

    [HttpGet("{orderId:guid}/items")]
    public async Task<ActionResult<IReadOnlyList<OrderItemResponse>>> GetOrderItemsAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPatch("{orderId:guid}")]
    public async Task<ActionResult<OrderModel>> UpdateStatusAsync([NotEmptyGuid] Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderModel>> CancelAsync([NotEmptyGuid] Guid orderId, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{orderId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetReviewsByOrderAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{orderId:guid}/shipments")]
    public async Task<ActionResult<IReadOnlyList<ShipmentResponse>>> GetShipmentsByOrderAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost("{orderId:guid}/shipments")]
    public async Task<ActionResult<ShipmentModel>> RecordShipmentAsync([NotEmptyGuid] Guid orderId, [FromBody] RecordShipmentRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        if (_currentUserProvider.UserId is { } currentUserId)
        {
            userId = currentUserId;
            return true;
        }

        userId = Guid.Empty;
        return false;
    }
}
