using Backend.Api.Contracts.Operation.Analytics;
using Backend.Api.Contracts.Common;
using Backend.Api.Mappings.Operation.Analytics;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AnalyticsController(IAnalyticsService analyticsService, ICurrentUserProvider currentUserProvider)
    {
        _analyticsService = analyticsService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesStatisticsResponse>> GetSalesStatisticsAsync(
        [FromQuery] GetSalesStatisticsRequest request,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view analytics." });
        }

        var applicationRequest = request.ToApplicationRequest(pageRequest.Page, pageRequest.PageSize);
        var result = await _analyticsService.GetSalesStatisticsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }

    [HttpGet("orders")]
    public async Task<ActionResult<OrdersStatisticsResponse>> GetOrdersStatisticsAsync(
        [FromQuery] GetOrdersStatisticsRequest request,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Only admins can view analytics." });
        }

        var applicationRequest = request.ToApplicationRequest(pageRequest.Page, pageRequest.PageSize);
        var result = await _analyticsService.GetOrderStatisticsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }
}
