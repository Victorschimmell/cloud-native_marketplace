using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Mappings.Commerce.Checkout;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/checkout")]
public class CheckoutController : ApiControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ICheckoutService checkoutService, ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> CheckoutAsync([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("preview")]
    public async Task<ActionResult<IReadOnlyList<CheckoutPreviewLineResponse>>> GetCheckoutPreviewAsync([FromQuery] CheckoutPreviewRequest request, [FromQuery] string currency, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Checkout preview requested for cart {CartId}, user {UserId}, session {SessionId}, display currency {Currency}.",
            request.CartId,
            request.UserId,
            request.SessionId,
            currency);

        var result = await _checkoutService.GetCheckoutPreviewAsync(request.ToApplicationRequest(), currency, cancellationToken);
        return HandleResult(result, lines => lines.Select(line => line.ToResponse()).ToArray());
    }
}
