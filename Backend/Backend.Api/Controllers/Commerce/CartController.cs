using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Mappings.Commerce.Cart;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ActionResult<CartResponse>> AddCartItemAsync([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Add cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            request.ListingId,
            request.Quantity,
            request.CartId,
            request.UserId,
            request.SessionId);

        var result = await _cartService.AddItemAsync(request.ToApplicationRequest(), cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpDelete]
    public async Task<ActionResult<CartResponse>> RemoveCartItemAsync([FromBody] RemoveCartItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Remove cart item requested for listing {ListingId}, quantity {Quantity}, cart {CartId}, user {UserId}, session {SessionId}.",
            request.ListingId,
            request.Quantity,
            request.CartId,
            request.UserId,
            request.SessionId);

        var result = await _cartService.RemoveItemAsync(request.ToApplicationRequest(), cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }
}
