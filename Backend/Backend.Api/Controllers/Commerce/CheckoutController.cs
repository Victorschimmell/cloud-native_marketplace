using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/checkout")]
public class CheckoutController : ApiControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> CheckoutAsync([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("preview")]
    public async Task<ActionResult<IReadOnlyList<CheckoutPreviewLineResponse>>> GetCheckoutPreviewAsync([FromQuery] CheckoutPreviewRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
