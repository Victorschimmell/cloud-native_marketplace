using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Api.Mappings.Common;
using Backend.Api.Mappings.User.Registration;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/customers")]
[Authorize]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        IOrderService orderService,
        ICurrentUserProvider currentUserProvider,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _orderService = orderService;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerResponse>> GetCustomersAsync([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can access customers." });
        }

        _logger.LogInformation(
            "Fetching customers with page {Page} and page size {PageSize}.",
            pageRequest.Page,
            pageRequest.PageSize);

        var result = await _customerService.GetCustomersAsync(pageRequest.ToAppRequest(), cancellationToken);
        return HandleResult(result, customer => customer.ToResponse());
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetByIdAsync([NotEmptyGuid] Guid userId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var authenticatedUserId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        if (!_currentUserProvider.IsAdmin && authenticatedUserId != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Cannot access another customer." });
        }

        _logger.LogInformation("Fetching customer with ID {userId}.", userId);

        var result = await _customerService.GetByIdAsync(userId, cancellationToken);
        return HandleResult(result, customer => customer.ToResponse());
    }

    [HttpGet("{userId:guid}/orders")]
    public async Task<ActionResult<PageResponse<OrderModel>>> GetOrdersByCustomerAsync(
        [NotEmptyGuid] Guid userId,
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var authenticatedUserId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        if (authenticatedUserId != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Cannot access orders for another customer." });
        }

        var result = await _orderService.GetByCustomerUserAsync(userId, pageRequest.ToAppRequest(), currency, cancellationToken);
        return HandleResult(
            result,
            page => new PageResponse<OrderModel>
            {
                Items = page.Items.Select(order => order.ToModel()).ToArray(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            });
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
