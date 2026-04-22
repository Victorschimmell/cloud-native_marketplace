using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);

        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
    }

    public Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<OrderDto>>.NotImplemented());
    }

    public Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<OrderItemDto>>.NotImplemented());
    }

    public Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }
}
