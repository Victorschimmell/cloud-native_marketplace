using Backend.Api.Contracts.Operation.Analytics;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Operation.Analytics;

public static class AnalyticsMappingExtensions
{
    public static App.GetSalesStatisticsRequest ToApplicationRequest(this GetSalesStatisticsRequest request, int page, int pageSize) =>
        new(request.FromUtc, request.ToUtc, page, pageSize);

    public static App.GetOrderStatisticsRequest ToApplicationRequest(this GetOrdersStatisticsRequest request, int page, int pageSize) =>
        new(request.FromUtc, request.ToUtc, page, pageSize);

    public static SalesStatisticsResponse ToResponse(this App.SalesStatisticsDto dto) =>
        new()
        {
            OrderCount = dto.OrderCount,
            TotalSalesAmount = dto.TotalSalesAmount,
            AverageOrderValue = dto.AverageOrderValue,
            GeneratedAtUtc = dto.GeneratedAtUtc
        };

    public static OrdersStatisticsResponse ToResponse(this App.OrderStatisticsDto dto) =>
        new()
        {
            TotalOrders = dto.TotalOrders,
            CancelledOrders = dto.CancelledOrders,
            CompletedOrders = dto.CompletedOrders,
            GeneratedAtUtc = dto.GeneratedAtUtc
        };
}
