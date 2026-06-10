using Backend.Api.Attributes;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.User.Registration;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.User;

[Route("api/sellers")]
public class SellersController : ApiControllerBase
{
    private readonly ISellerService _sellerService;

    public SellersController(ISellerService sellerService)
    {
        _sellerService = sellerService;
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
}
