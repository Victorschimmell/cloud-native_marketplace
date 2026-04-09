namespace Backend.Application.DTOs;

public sealed record GetSalesStatisticsRequest(DateTimeOffset? FromUtc, DateTimeOffset? ToUtc, int Page = 1, int PageSize = 100);

public sealed record GetOrderStatisticsRequest(DateTimeOffset? FromUtc, DateTimeOffset? ToUtc, int Page = 1, int PageSize = 100);

public sealed record SalesStatisticsDto(
    int OrderCount,
    decimal TotalSalesAmount,
    decimal AverageOrderValue,
    DateTimeOffset GeneratedAtUtc);

public sealed record OrderStatisticsDto(
    int TotalOrders,
    int CancelledOrders,
    int CompletedOrders,
    DateTimeOffset GeneratedAtUtc);

public sealed record AnalyticsResponse(SalesStatisticsDto? Sales, OrderStatisticsDto? Orders);
