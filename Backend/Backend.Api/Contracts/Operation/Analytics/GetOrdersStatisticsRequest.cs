namespace Backend.Api.Contracts.Operation.Analytics;

public sealed record GetOrdersStatisticsRequest
{
    public DateTimeOffset? FromUtc { get; init; }
    public DateTimeOffset? ToUtc { get; init; }
}
