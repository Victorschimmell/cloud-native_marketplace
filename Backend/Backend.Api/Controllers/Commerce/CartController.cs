using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Mappings.Commerce.Cart;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;

namespace Backend.Api.Controllers.Commerce;

[Route("api/cart")]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    // TODO: Using get identity after authentication is implemented instead of passing userId in query parameters
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<CartResponse>> GetCartAsync([NotEmptyGuid] Guid userId, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(new App.GetCartRequest(null, userId, null), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddCartItemAsync([FromBody] AddCartItemRequest request, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Add cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            request.ListingId,
            request.Quantity,
            request.CartId,
            request.UserId,
            request.SessionId);

        var result = await _cartService.AddItemAsync(request.ToApplicationRequest(), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpPatch("items/{listingId}")]
    public async Task<ActionResult<CartResponse>> UpdateCartItemAsync([NotEmptyGuid] Guid listingId, [FromBody] UpdateCartItemRequest request, [FromQuery] string displayCurrency, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Update cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            listingId,
            request.Quantity,
            request.CartId,
            request.UserId,
            request.SessionId);

        var result = await _cartService.UpdateItemAsync(request.ToApplicationRequest(listingId), displayCurrency, cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }
}
