using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Contracts.Commerce.Orders;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Api.Mappings.Commerce.Cart;
using Backend.Api.Mappings.Common;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using App = Backend.Application.DTOs;

namespace Backend.Api.Controllers.User;

[Route("api/customers")]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICartService _cartService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ICartService cartService,ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _cartService = cartService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerResponse>> GetCustomersAsync([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching customers with page {Page} and page size {PageSize}.",
            pageRequest.Page,
            pageRequest.PageSize);

        var result = await _customerService.GetCustomersAsync(pageRequest.ToAppRequest(), cancellationToken);
        return HandleResult(result, customer => customer.ToResponse());
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<PageResponse<CustomerResponse>>> GetByIdAsync([NotEmptyGuid] Guid customerId, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{customerId:guid}/cart")]
    public async Task<ActionResult<CartResponse>> GetCartAsync([NotEmptyGuid] Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(new App.GetCartRequest(null, customerId, null), cancellationToken);
        return HandleResult(result, cart => cart.ToResponse());
    }

    [HttpGet("{customerId:guid}/orders")]
    public async Task<ActionResult<PageResponse<OrderResponse>>> GetOrdersByCustomerAsync([NotEmptyGuid] Guid customerId, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
