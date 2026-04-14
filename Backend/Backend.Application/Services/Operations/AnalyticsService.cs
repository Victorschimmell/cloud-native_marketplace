using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AnalyticsService(IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            var orders = await _orderRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var filteredOrders = ApplyDateFilter(orders, request.FromUtc, request.ToUtc);
            var totalSales = filteredOrders.Sum(order => order.TotalAmount);
            var count = filteredOrders.Count;

            var statistics = new SalesStatisticsDto(
                count,
                totalSales,
                count == 0 ? 0m : totalSales / count,
                _dateTimeProvider.UtcNow);

            return Result<SalesStatisticsDto>.Success(statistics);
        }, "Unable to get sales statistics.");
    }

    public async Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            var orders = await _orderRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var filteredOrders = ApplyDateFilter(orders, request.FromUtc, request.ToUtc);

            var statistics = new OrderStatisticsDto(
                filteredOrders.Count,
                filteredOrders.Count(order => order.OrderStatus == OrderStatus.Cancelled),
                filteredOrders.Count(order => order.OrderStatus == OrderStatus.Delivered),
                _dateTimeProvider.UtcNow);

            return Result<OrderStatisticsDto>.Success(statistics);
        }, "Unable to get order statistics.");
    }

    private static List<Order> ApplyDateFilter(IReadOnlyList<Order> orders, DateTimeOffset? fromUtc, DateTimeOffset? toUtc)
    {
        return orders
            .Where(order => !fromUtc.HasValue || order.OrderPurchaseTimestampUtc >= fromUtc.Value)
            .Where(order => !toUtc.HasValue || order.OrderPurchaseTimestampUtc <= toUtc.Value)
            .ToList();
    }
}
