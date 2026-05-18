using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserProvider _currentUserProvider;

    public AnalyticsService(
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserProvider currentUserProvider)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<SalesStatisticsDto>.Forbidden("Only admins can view sales statistics.");
        }

        if (request.FromUtc.HasValue && request.ToUtc.HasValue && request.FromUtc > request.ToUtc)
        {
            return Result<SalesStatisticsDto>.ValidationFailure("FromUtc must be earlier than or equal to ToUtc.");
        }

        var aggregate = await _orderRepository.GetSalesAggregateAsync(request.FromUtc, request.ToUtc, cancellationToken);
        var averageOrderValue = aggregate.OrderCount == 0
            ? 0m
            : Math.Round(aggregate.TotalSalesAmount / aggregate.OrderCount, 2);

        return Result<SalesStatisticsDto>.Success(new SalesStatisticsDto(
            aggregate.OrderCount,
            aggregate.TotalSalesAmount,
            averageOrderValue,
            _dateTimeProvider.UtcNow));
    }

    public async Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<OrderStatisticsDto>.Forbidden("Only admins can view order statistics.");
        }

        if (request.FromUtc.HasValue && request.ToUtc.HasValue && request.FromUtc > request.ToUtc)
        {
            return Result<OrderStatisticsDto>.ValidationFailure("FromUtc must be earlier than or equal to ToUtc.");
        }

        var aggregate = await _orderRepository.GetOrderStatusAggregateAsync(request.FromUtc, request.ToUtc, cancellationToken);
        return Result<OrderStatisticsDto>.Success(new OrderStatisticsDto(
            aggregate.TotalOrders,
            aggregate.CancelledOrders,
            aggregate.CompletedOrders,
            _dateTimeProvider.UtcNow));
    }
}
