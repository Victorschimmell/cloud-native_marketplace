using Backend.Api.Attributes;
using Backend.Api.Contracts.Commerce.Shipments;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Commerce;

[Route("api/shipments")]
public class ShipmentsController : ApiControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpPatch("{shipmentId:guid}/status")]
    public async Task<ActionResult<ShipmentResponse>> UpdateShipmentStatusAsync([NotEmptyGuid] Guid shipmentId, [FromBody] UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
