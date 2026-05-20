using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Api.Mappings.Commerce.Orders;
using Backend.Api.Mappings.Common;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiOrderStatus = Backend.Api.Contracts.Commerce.Orders.OrderStatus;
using DomainOrderStatus = Backend.Domain.Enums.OrderStatus;

namespace Backend.Api.Controllers.User;

[Route("api/sellers")]
public class SellersController : ApiControllerBase
{
    private readonly ISellerService _sellerService;
    private readonly IOrderService _orderService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SellersController(
        ISellerService sellerService,
        IOrderService orderService,
        ICurrentUserProvider currentUserProvider)
    {
        _sellerService = sellerService;
        _orderService = orderService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<ActionResult<SellerResponse>> GetSellersAsync([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        // var result = await _customerService.GetCustomersAsync(request.ToDto());
        // return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{sellerId:guid}")]
    public async Task<ActionResult<PageResponse<SellerResponse>>> GetByIdAsync([NotEmptyGuid] Guid sellerId, CancellationToken cancellationToken)
    {
        // var result = await _customerService.GetByIdAsync(customerId);
        // return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [Authorize]
    [HttpGet("me/orders")]
    public async Task<ActionResult<PageResponse<SellerOrderSummaryModel>>> GetMyOrdersAsync(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? currency,
        [FromQuery] ApiOrderStatus? status,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var authenticatedUserId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        if (!TryParseOrderSort(sort, out var orderSort))
        {
            return BadRequest(new { Error = $"Unsupported order sort '{sort}'." });
        }

        var result = await _orderService.GetSummaryBySellerUserAsync(
            authenticatedUserId,
            pageRequest.ToAppRequest(),
            currency,
            status.HasValue ? (DomainOrderStatus)(int)status.Value : null,
            orderSort,
            cancellationToken);

        return HandleResult(
            result,
            page => new PageResponse<SellerOrderSummaryModel>
            {
                Items = page.Items.Select(order => order.ToSellerSummaryModel()).ToArray(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            });
    }

    [Authorize]
    [HttpGet("me/order-stats")]
    public async Task<ActionResult<SellerOrderStatsModel>> GetMyOrderStatsAsync(
        [FromQuery] string? currency,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var authenticatedUserId))
        {
            return Unauthorized(new { Error = "Authenticated user id is missing." });
        }

        var result = await _orderService.GetStatsBySellerUserAsync(authenticatedUserId, currency, cancellationToken);
        return HandleResult(result, stats => stats.ToSellerStatsModel());
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

    private static bool TryParseOrderSort(string? sort, out SellerOrderSort orderSort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            orderSort = SellerOrderSort.Newest;
            return true;
        }

        orderSort = sort.Trim().ToLowerInvariant() switch
        {
            "newest" => SellerOrderSort.Newest,
            "oldest" => SellerOrderSort.Oldest,
            "total-high" => SellerOrderSort.TotalHigh,
            "total-low" => SellerOrderSort.TotalLow,
            _ => SellerOrderSort.Newest
        };

        return sort.Trim().Equals("newest", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("oldest", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("total-high", StringComparison.OrdinalIgnoreCase)
            || sort.Trim().Equals("total-low", StringComparison.OrdinalIgnoreCase);
    }
}
