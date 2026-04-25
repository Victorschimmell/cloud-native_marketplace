using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
namespace Backend.Application.Services;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CheckoutService(ICartRepository cartRepository, IOrderRepository orderRepository, IPaymentRepository paymentRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<CheckoutLineDto>>.NotImplemented());
    }

    public Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CheckoutResponse>.NotImplemented());
    }
}
