using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class AdminDashboardRepository(ApplicationDbContext dbContext) : IAdminDashboardRepository
{
    public async Task<AdminDashboardSnapshot> GetSnapshotAsync(
        DateTimeOffset ordersFromUtc,
        DateTimeOffset ordersToUtc,
        CancellationToken cancellationToken = default)
    {
        var activeUsers = await dbContext.UserAccounts
            .AsNoTracking()
            .CountAsync(u => !u.IsBlocked && u.AccountStatus == AccountStatus.Active, cancellationToken);

        var ordersInLast24Hours = await dbContext.Orders
            .AsNoTracking()
            .CountAsync(
                o => o.OrderPurchaseTimestampUtc >= ordersFromUtc
                    && o.OrderPurchaseTimestampUtc <= ordersToUtc,
                cancellationToken);

        var totalRevenue = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.OrderStatus != OrderStatus.Cancelled && o.OrderStatus != OrderStatus.Returned)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var unresolvedIssues = await dbContext.AdminIssues
            .AsNoTracking()
            .CountAsync(i => i.Status != IssueStatus.Resolved, cancellationToken);

        return new AdminDashboardSnapshot(activeUsers, ordersInLast24Hours, totalRevenue, unresolvedIssues);
    }
}
