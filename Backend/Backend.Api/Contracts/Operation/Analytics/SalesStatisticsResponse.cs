namespace Backend.Api.Contracts.Operation.Analytics;

public sealed record SalesStatisticsResponse
{
    public required int OrderCount { get; init; }
    public required decimal TotalSalesAmount { get; init; }
    public required decimal AverageOrderValue { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}
