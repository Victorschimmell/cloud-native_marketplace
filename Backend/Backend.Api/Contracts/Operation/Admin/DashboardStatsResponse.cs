namespace Backend.Api.Contracts.Operation.Admin;

public sealed record DashboardStatsResponse
{
    public required int ActiveUsers { get; init; }
    public required int OrdersInLast24Hours { get; init; }
    public required decimal TotalRevenue { get; init; }
    public required string CurrencyCode { get; init; }
    public required int OpenIssues { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}
