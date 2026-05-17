namespace Backend.Api.Contracts.Operation.Analytics;

public sealed record OrdersStatisticsResponse
{
    public required int TotalOrders { get; init; }
    public required int CancelledOrders { get; init; }
    public required int CompletedOrders { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}
