using Backend.Api.Auth;
using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.Analytics;
using Backend.Api.Mappings.Operation.Analytics;
using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers.Operations;

[Route("api/analytics")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesStatisticsResponse>> GetSalesStatisticsAsync(
        [FromQuery] GetSalesStatisticsRequest request,
        [FromQuery] PageRequest pageRequest,
        CancellationToken cancellationToken)
    {
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
        var applicationRequest = request.ToApplicationRequest(pageRequest.Page, pageRequest.PageSize);
        var result = await _analyticsService.GetOrderStatisticsAsync(applicationRequest, cancellationToken);
        return HandleResult(result, dto => dto.ToResponse());
    }
}
