using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Commerce.Reviews;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Api.Mappings.Commerce.Reviews;
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
    private readonly ICurrentUserProvider _currentUserProvider;

    public OrdersController(
        IOrderService orderService,
        IReviewService reviewService,
        ICurrentUserProvider currentUserProvider)
    {
        _orderService = orderService;
        _reviewService = reviewService;
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

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderModel>> CancelAsync([NotEmptyGuid] Guid orderId, [FromBody] CancelOrderRequest request, [FromQuery] string? currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var authenticatedUserId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _orderService.CancelAsync(request.ToApplicationRequest(orderId), authenticatedUserId, currency, cancellationToken);
        return HandleResult(result, order => order.ToModel());
    }

    [HttpGet("{orderId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetReviewsByOrderAsync([NotEmptyGuid] Guid orderId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _reviewService.GetByOrderAsync(orderId, userId, cancellationToken);
        return HandleResult(result, reviews => reviews.Select(review => review.ToResponse()).ToArray());
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
