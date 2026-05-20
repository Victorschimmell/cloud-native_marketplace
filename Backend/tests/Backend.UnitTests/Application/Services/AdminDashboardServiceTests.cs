using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class AdminDashboardServiceTests
{
    [Fact]
    public async Task GetDashboardStatsAsync_Uses_Unresolved_Issue_Count()
    {
        var dashboardRepository = new FakeAdminDashboardRepository
        {
            Snapshot = new(ActiveUsers: 3, OrdersInLast24Hours: 4, TotalRevenue: 100m, UnresolvedIssues: 2)
        };
        var service = CreateService(dashboardRepository);

        var result = await service.GetDashboardStatsAsync(
            new GetDashboardStatsRequest("BRL"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.ActiveUsers);
        Assert.Equal(4, result.Value.OrdersInLast24Hours);
        Assert.Equal(100m, result.Value.TotalRevenue);
        Assert.Equal(2, result.Value.UnresolvedIssues);
    }

    private static AdminDashboardService CreateService(FakeAdminDashboardRepository dashboardRepository)
    {
        return new AdminDashboardService(
            dashboardRepository,
            new FakeCurrencyConversionService(),
            new FakeDateTimeProvider(),
            new FakeCurrentUserProvider());
    }
}
