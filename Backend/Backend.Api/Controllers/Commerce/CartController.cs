using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Mappings.Commerce.Cart;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;

namespace Backend.Api.Controllers.Commerce;

[Route("api/cart")]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartService cartService,
        ICurrentUserProvider currentUserProvider,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    [HttpGet("{cartId:guid}")]
    public async Task<ActionResult<CartResponse>> GetCartAsync([NotEmptyGuid] Guid cartId, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _cartService.GetCartAsync(new App.GetCartRequest(cartId, userId, null), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpGet("current")]
    public async Task<ActionResult<CartResponse>> GetCurrentCartAsync([FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _cartService.GetCartAsync(new App.GetCartRequest(null, userId, null), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddCartItemAsync([FromBody] AddCartItemRequest request, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation(
            "Add cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            request.ListingId,
            request.Quantity,
            request.CartId,
            userId,
            null);

        var result = await _cartService.AddItemAsync(request.ToApplicationRequest(userId), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpPatch("items/{listingId}")]
    public async Task<ActionResult<CartResponse>> UpdateCartItemAsync([NotEmptyGuid] Guid listingId, [FromBody] UpdateCartItemRequest request, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation(
            "Update cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            listingId,
            request.Quantity,
            request.CartId,
            userId,
            null);

        var result = await _cartService.UpdateItemAsync(request.ToApplicationRequest(listingId, userId), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
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
