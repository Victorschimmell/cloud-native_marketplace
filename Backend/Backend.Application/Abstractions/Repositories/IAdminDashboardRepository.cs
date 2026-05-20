namespace Backend.Application.Abstractions.Repositories;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardSnapshot> GetSnapshotAsync(
        DateTimeOffset ordersFromUtc,
        DateTimeOffset ordersToUtc,
        CancellationToken cancellationToken = default);
}

public sealed record AdminDashboardSnapshot(
    int ActiveUsers,
    int OrdersInLast24Hours,
    decimal TotalRevenue,
    int UnresolvedIssues);
