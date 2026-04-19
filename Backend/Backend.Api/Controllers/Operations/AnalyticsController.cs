using Backend.Api.Contracts.Operation.Analytics;
using Backend.Api.Contracts.Common;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/analytics")]
public class AnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesStatisticsResponse>> GetSalesStatisticsAsync([FromQuery] GetSalesStatisticsRequest request, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }

    [HttpGet("orders")]
    public async Task<ActionResult<OrdersStatisticsResponse>> GetOrdersStatisticsAsync([FromQuery] GetOrdersStatisticsRequest request, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "This endpoint is not implemented yet.");
    }
}
