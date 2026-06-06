using Backend.Api.Attributes;
using Backend.Api.Auth;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Api.Mappings.Common;
using Backend.Api.Mappings.User.Registration;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Enums;
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
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<PageResponse<CustomerResponse>>> GetCustomersAsync([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching customers with page {Page} and page size {PageSize}.",
            pageRequest.Page,
            pageRequest.PageSize);

        var result = await _customerService.GetCustomersAsync(pageRequest.ToAppRequest(), cancellationToken);
        return HandleResult(
            result,
            page => new PageResponse<CustomerResponse>
            {
                Items = page.Items.Select(customer => customer.ToResponse()).ToArray(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            });
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

        _logger.LogInformation("Fetching customer with ID {UserId}.", userId);

        var result = await _customerService.GetByIdAsync(userId, cancellationToken);
        return HandleResult(result, customer => customer.ToResponse());
    }

    [HttpGet("{userId:guid}/orders")]
    public async Task<ActionResult<PageResponse<OrderSummaryModel>>> GetOrdersSummaryByCustomerAsync(
        [NotEmptyGuid] Guid userId,
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? currency,
        [FromQuery] OrderStatus? status,
        [FromQuery] string? sort,
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

        if (!TryParseOrderSort(sort, out var orderSort))
        {
            return BadRequest(new { Error = $"Unsupported order sort '{sort}'." });
        }

        var result = await _orderService.GetSummaryByCustomerUserAsync(
            userId,
            pageRequest.ToAppRequest(),
            currency,
            status,
            orderSort,
            cancellationToken);
        return HandleResult(
            result,
            page => new PageResponse<OrderSummaryModel>
            {
                Items = page.Items.Select(order => order.ToSummaryModel()).ToArray(),
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

    private static bool TryParseOrderSort(string? sort, out CustomerOrderSort orderSort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            orderSort = CustomerOrderSort.Newest;
            return true;
        }

        orderSort = sort.Trim().ToLowerInvariant() switch
        {
            "newest" => CustomerOrderSort.Newest,
            "oldest" => CustomerOrderSort.Oldest,
            "total-high" => CustomerOrderSort.TotalHigh,
            "total-low" => CustomerOrderSort.TotalLow,
            _ => CustomerOrderSort.Newest
        };

        return sort.Trim().Equals("newest", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("oldest", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("total-high", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("total-low", StringComparison.OrdinalIgnoreCase);
    }
}
