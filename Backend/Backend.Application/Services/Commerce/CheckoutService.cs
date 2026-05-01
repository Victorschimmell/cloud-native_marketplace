using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentService _paymentService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IOrderNumberGenerator orderNumberGenerator,
        ICustomerRepository customerRepository,
        IPaymentService paymentService,
        IDateTimeProvider dateTimeProvider,
        ICurrencyConversionService currencyConversionService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderNumberGenerator);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(paymentService);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _orderNumberGenerator = orderNumberGenerator;
        _customerRepository = customerRepository;
        _paymentService = paymentService;
        _dateTimeProvider = dateTimeProvider;
        _currencyConversionService = currencyConversionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.NotFound("Cart was not found for the provided identifiers.");
        }

        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var checkoutLines = cart.Items.Select(item => new CheckoutLineDto(
            item.ListingId,
            item.Quantity,
            priceConverter(item.UnitPriceAtAddition),
            priceConverter(item.UnitPriceAtAddition * item.Quantity),
            currencyCode)).ToArray();

        return Result<IReadOnlyList<CheckoutLineDto>>.Success(checkoutLines);
    }

    public async Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<CheckoutResponse>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<CheckoutResponse>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<CheckoutResponse>.NotFound("Cart was not found for the provided identifiers.");
        }

        var now = _dateTimeProvider.UtcNow;
        var orderNumber = await _orderNumberGenerator.GenerateOrderNumberAsync(cancellationToken);

        // 1. Create Order
        // TODO: Decide whether to use cart.UserId or userId from the request to identify/varify the correct customer and their cart
        // TODO: Temporarily use the userId from cart, after authentication is implemented we can have a proper implementation here
        var customer = await _customerRepository.GetByUserIdAsync(cart.UserId.Value, cancellationToken);
        var customerId = customer.Id;
        var subtotalAmount = cart.Items.Sum(i => i.UnitPriceAtAddition * i.Quantity);
        var freightAmount = 100m;  // TODO: Implement proper freight calculation, currently using a fixed amount
        var order = new Order
        {
            CustomerId = customerId,
            ShippingAddressId = request.ShippingAddressId,
            OrderStatus = OrderStatus.Pending,
            OrderPurchaseTimestampUtc = now,
            SubtotalAmount = subtotalAmount,
            FreightAmount = freightAmount,
            TotalAmount = subtotalAmount + freightAmount,
            PlacedFromCartId = cart.Id,
            OrderNumber = orderNumber
        };
        await _orderRepository.AddAsync(order, cancellationToken);

        // 2. process payments
        var payments = new List<PaymentDto>();
        foreach (var paymentRequest in request.Payments)
        {
            var paymentRequestWithOrderId = new RecordPaymentRequest(order.Id, paymentRequest);
            // NOTE: Currently never fails
            var paymentResult = await _paymentService.RecordPaymentAsync(paymentRequestWithOrderId, cancellationToken);
            if (!paymentResult.IsSuccess)
            {
                // TODO: Implement rollback mechanism to undo the created order in case of payment failure, currently always success
                // await _unitOfWork.RollbackAsync(cancellationToken);
                return Result<CheckoutResponse>.Failure("Payment processing failed: " + paymentResult.Error);
            }

            if (paymentResult.Value is null)
            {
                return Result<CheckoutResponse>.Failure("Payment processing failed: payment result was null.");
            }
            payments.Add(paymentResult.Value);
        }

        // 3. Update cart and order status
        cart.Status = CartStatus.Converted;
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        order.OrderStatus = OrderStatus.Approved;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // 4. Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Return response
        var response = new CheckoutResponse(
            order.ToOrderDto(currencyCode, priceConverter),
            cart.ToCartDto(currencyCode, priceConverter),
            payments,
            priceConverter(order.TotalAmount),
            currencyCode);
        return Result<CheckoutResponse>.Success(response);
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

    private async Task<ShoppingCart?> GetActiveCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = await GetCartAsync(CartId, UserId, SessionId, cancellationToken);

        if (cart is { Status: CartStatus.Active })
        {
            // cart expired
            if (cart.ExpiresAtUtc < _dateTimeProvider.UtcNow)
            {
                cart.Status = CartStatus.Expired;
                await _cartRepository.UpdateAsync(cart, cancellationToken);
                return null;
            }

            var now = _dateTimeProvider.UtcNow;
            cart.ExpiresAtUtc = now.AddDays(14);
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            return cart;
        }

        return null;
    }
}
