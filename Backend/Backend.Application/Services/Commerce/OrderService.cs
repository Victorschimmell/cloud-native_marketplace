using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Enums;

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

    public async Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (orderId == Guid.Empty)
            {
                return Result<OrderDto>.ValidationFailure("Order id is required.");
            }

            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            return order is null
                ? Result<OrderDto>.NotFound("Order was not found.")
                : Result<OrderDto>.Success(order.ToDto());
        }, "Unable to get order.");
    }

    public async Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (customerId == Guid.Empty)
            {
                return Result<PagedResult<OrderDto>>.ValidationFailure("Customer id is required.");
            }

            if (request.Page <= 0 || request.PageSize <= 0)
            {
                return Result<PagedResult<OrderDto>>.ValidationFailure("Page and page size must be greater than zero.");
            }

            var orders = await _orderRepository.GetByCustomerIdAsync(customerId, request.Page, request.PageSize, cancellationToken);
            var items = orders.Select(static order => order.ToDto()).ToArray();
            return Result<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>(items, request.Page, request.PageSize, items.Length));
        }, "Unable to get orders by customer.");
    }

    public async Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (orderId == Guid.Empty)
            {
                return Result<IReadOnlyList<OrderItemDto>>.ValidationFailure("Order id is required.");
            }

            var items = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
            return Result<IReadOnlyList<OrderItemDto>>.Success(items.Select(static item => item.ToDto()).ToArray());
        }, "Unable to get order items.");
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.OrderId == Guid.Empty)
            {
                return Result<OrderDto>.ValidationFailure("Order id is required.");
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result<OrderDto>.NotFound("Order was not found.");
            }

            order.OrderStatus = request.Status;
            await _orderRepository.UpdateAsync(order, cancellationToken);
            return Result<OrderDto>.Success(order.ToDto());
        }, "Unable to update order status.");
    }

    public async Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.OrderId == Guid.Empty)
            {
                return Result<OrderDto>.ValidationFailure("Order id is required.");
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                return Result<OrderDto>.NotFound("Order was not found.");
            }

            order.OrderStatus = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order, cancellationToken);
            return Result<OrderDto>.Success(order.ToDto());
        }, "Unable to cancel order.");
    }
}
