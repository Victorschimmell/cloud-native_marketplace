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

    public AnalyticsService(IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<SalesStatisticsDto>> GetSalesStatisticsAsync(GetSalesStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<OrderStatisticsDto>> GetOrderStatisticsAsync(GetOrderStatisticsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
