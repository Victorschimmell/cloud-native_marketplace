using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default);
}
