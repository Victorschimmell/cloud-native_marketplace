using Backend.Api.Attributes;
using Backend.Api.Contracts.User.Addresses;
using Backend.Api.Mappings.User.Addresses;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/addresses")]
[Authorize]
public class AddressesController : ApiControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        IAddressService addressService,
        ICurrentUserProvider currentUserProvider,
        ILogger<AddressesController> logger)
    {
        _addressService = addressService;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    [HttpGet("{addressId:guid}")]
    public async Task<ActionResult<AddressResponse>> GetByIdAsync([NotEmptyGuid] Guid addressId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation("Fetching address {AddressId} for user {UserId}.", addressId, userId);

        var result = await _addressService.GetByIdAsync(addressId, userId, cancellationToken);
        return HandleResult(result, address => address.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<AddressResponse>> CreateAsync([FromBody] CreateAddressRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        _logger.LogInformation("Creating address for user {UserId}.", userId);

        var result = await _addressService.CreateAsync(request.ToApplicationRequest(), userId, cancellationToken);
        return HandleResult(result, address => address.ToResponse());
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
