using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Mappings.Commerce.Checkout;
using Backend.Application.Abstractions.Repositories;
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
    private readonly ICustomerRepository _customerRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        ICheckoutService checkoutService,
        ICurrentUserProvider currentUserProvider,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService;
        _currentUserProvider = currentUserProvider;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _logger = logger;
    }

    [HttpGet("preview")]
    public async Task<ActionResult<CheckoutPreviewResponse>> GetCheckoutPreviewAsync([FromQuery] CheckoutPreviewRequest request, [FromQuery] string currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        if (await GetBuyerRestrictionAsync(userId, cancellationToken) is { } restriction)
        {
            return restriction;
        }

        _logger.LogInformation(
            "Checkout preview requested for cart {CartId}, user {UserId}, session {SessionId}, display currency {Currency}.",
            request.CartId,
            userId,
            null,
            currency);

        var result = await _checkoutService.GetCheckoutPreviewAsync(request.ToApplicationRequest(userId), currency, cancellationToken);
        return HandleResult(result, preview => preview.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> CheckoutAsync([FromBody] CheckoutRequest request, string currency, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        if (await GetBuyerRestrictionAsync(userId, cancellationToken) is { } restriction)
        {
            return restriction;
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

    private async Task<ActionResult?> GetBuyerRestrictionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (seller is not null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Seller accounts cannot check out or buy products." });
        }

        var customer = await _customerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only customer accounts can check out." });
        }

        return null;
    }
}
