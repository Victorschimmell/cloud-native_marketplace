using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Commerce.Shipments;
using Backend.Api.Contracts.Common;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/orders")]
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IReviewService _reviewService;
    private readonly IShipmentService _shipmentService;

    public OrdersController(IOrderService orderService, IReviewService reviewService, IShipmentService shipmentService)
    {
        _orderService = orderService;
        _reviewService = reviewService;
        _shipmentService = shipmentService;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderResponse>> GetByIdAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet]
    public async Task<ActionResult<PageResponse<OrderResponse>>> GetByCustomerAsync([NotEmptyGuid][FromQuery] Guid customerId, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{orderId:guid}/items")]
    public async Task<ActionResult<IReadOnlyList<OrderItemResponse>>> GetOrderItemsAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPatch("{orderId:guid}")]
    public async Task<ActionResult<OrderResponse>> UpdateStatusAsync([NotEmptyGuid] Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderResponse>> CancelAsync([NotEmptyGuid] Guid orderId, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
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
}
