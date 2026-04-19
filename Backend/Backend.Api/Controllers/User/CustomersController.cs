using Backend.Api.Attributes;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/customers")]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerResponse>> GetCustomersAsync([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        // var result = await _customerService.GetCustomersAsync(request.ToDto());
        // return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<PageResponse<CustomerResponse>>> GetByIdAsync([NotEmptyGuid] Guid customerId, CancellationToken cancellationToken)
    {
        // var result = await _customerService.GetByIdAsync(customerId);
        // return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
