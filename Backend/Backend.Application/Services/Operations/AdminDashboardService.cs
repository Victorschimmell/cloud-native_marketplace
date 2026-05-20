using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AdminDashboardService(
        IAdminDashboardRepository adminDashboardRepository,
        ICurrencyConversionService currencyConversionService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserProvider currentUserProvider)
    {
        ArgumentNullException.ThrowIfNull(adminDashboardRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currentUserProvider);

        _adminDashboardRepository = adminDashboardRepository;
        _currencyConversionService = currencyConversionService;
        _dateTimeProvider = dateTimeProvider;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(GetDashboardStatsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<DashboardStatsDto>.Forbidden("Only admins can view dashboard stats.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(request.Currency, out var currencyCode, out var convert))
        {
            return Result<DashboardStatsDto>.ValidationFailure($"Unsupported currency '{request.Currency}'.");
        }

        var now = _dateTimeProvider.UtcNow;
        var dayAgo = now.AddDays(-1);
        var snapshot = await _adminDashboardRepository.GetSnapshotAsync(dayAgo, now, cancellationToken);
        var totalRevenue = Math.Round(convert(snapshot.TotalRevenue), 2);

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto(
            ActiveUsers: snapshot.ActiveUsers,
            OrdersInLast24Hours: snapshot.OrdersInLast24Hours,
            TotalRevenue: totalRevenue,
            CurrencyCode: currencyCode,
            UnresolvedIssues: snapshot.UnresolvedIssues,
            GeneratedAtUtc: now));
    }
}
