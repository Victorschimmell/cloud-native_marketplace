using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrencyConversionService _currencyConversionService;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICustomerRepository customerRepository,
        ICurrencyConversionService currencyConversionService)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(currencyConversionService);

        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _customerRepository = customerRepository;
        _currencyConversionService = currencyConversionService;
    }

    public Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<OrderDto>.NotImplemented());
    }

    public async Task<Result<OrderDto>> GetByIdForCustomerAsync(Guid orderId, Guid authenticatedUserId, string? currency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<OrderDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id)
        {
            return Result<OrderDto>.NotFound("Order was not found for the authenticated customer.");
        }

        return Result<OrderDto>.Success(order.ToOrderDto(authenticatedUserId, currencyCode, priceConverter));
    }

    public Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<PagedResult<OrderDto>>.NotImplemented());
    }

    public async Task<Result<PagedResult<OrderDto>>> GetByCustomerUserAsync(Guid authenticatedUserId, PagedRequest request, string? currency, CancellationToken cancellationToken = default)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return Result<PagedResult<OrderDto>>.ValidationFailure("Page and page size must be greater than zero.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(currency, out var currencyCode, out var priceConverter))
        {
            return Result<PagedResult<OrderDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<OrderDto>>.NotFound("Customer profile was not found for the authenticated user.");
        }

        var orders = await _orderRepository.GetByCustomerIdWithDetailsAsync(customer.Id, request.Page, request.PageSize, cancellationToken);
        var mappedOrders = orders.Items
            .Select(order => order.ToOrderDto(authenticatedUserId, currencyCode, priceConverter))
            .ToArray();

        return Result<PagedResult<OrderDto>>.Success(
            new PagedResult<OrderDto>(mappedOrders, orders.Page, orders.PageSize, orders.TotalCount));
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
