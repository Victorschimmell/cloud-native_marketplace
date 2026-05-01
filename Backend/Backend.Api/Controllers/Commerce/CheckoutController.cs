using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Mappings.Commerce.Checkout;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/checkout")]
[Authorize]
public class CheckoutController : ApiControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        ICheckoutService checkoutService,
        ICurrentUserProvider currentUserProvider,
        ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    [HttpGet("preview")]
    public async Task<ActionResult<IReadOnlyList<CheckoutPreviewLineResponse>>> GetCheckoutPreviewAsync([FromQuery] CheckoutPreviewRequest request, [FromQuery] string currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation(
            "Checkout preview requested for cart {CartId}, user {UserId}, session {SessionId}, display currency {Currency}.",
            request.CartId,
            userId,
            null,
            currency);

        var result = await _checkoutService.GetCheckoutPreviewAsync(request.ToApplicationRequest(userId), currency, cancellationToken);
        return HandleResult(result, lines => lines.Select(line => line.ToResponse()).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> CheckoutAsync([FromBody] CheckoutRequest request, string currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation(
            "Checkout requested for cart {CartId}, user {UserId}, session {SessionId}, display currency {Currency}.",
            request.CartId,
            userId,
            null,
            currency);

        var result = await _checkoutService.CheckoutAsync(request.ToApplicationRequest(userId), currency, cancellationToken);
        return HandleResult(result, response => response.ToResponse());
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
