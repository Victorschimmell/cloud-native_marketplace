using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
namespace Backend.Application.Services;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrencyConversionService _currencyConversionService;

    public CheckoutService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IDateTimeProvider dateTimeProvider,
        ICurrencyConversionService currencyConversionService)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currencyConversionService);

        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
        _currencyConversionService = currencyConversionService;
    }

    public async Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        var cart = await GetCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.NotFound("Cart was not found for the provided identifiers.");
        }

        var currencyCode = _currencyConversionService.NormalizeOrDefault(displayCurrency);
        if (!_currencyConversionService.IsSupported(currencyCode))
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }
        var priceConverter = new Func<decimal, decimal>(price => _currencyConversionService.FromBaseCurrency(price, currencyCode));

        var checkoutLines = cart.Items.Select(item => new CheckoutLineDto(
            item.ListingId,
            item.Quantity,
            priceConverter(item.UnitPriceAtAddition),
            priceConverter(item.UnitPriceAtAddition * item.Quantity),
            currencyCode)).ToArray();

        return Result<IReadOnlyList<CheckoutLineDto>>.Success(checkoutLines);
    }

    public Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CheckoutResponse>.NotImplemented());
    }

    private async Task<ShoppingCart?> GetCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdAsync(CartId.Value, cancellationToken);
        }

        if (cart is null && UserId.HasValue)
        {
            cart = await _cartRepository.GetActiveByUserIdAsync(UserId.Value, cancellationToken);
        }

        if (cart is null && SessionId.HasValue)
        {
            cart = await _cartRepository.GetActiveBySessionIdAsync(SessionId.Value, cancellationToken);
        }

        return cart;
    }
}
