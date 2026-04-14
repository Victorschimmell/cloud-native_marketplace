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

    public async Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            if (cart is null)
            {
                return Result<IReadOnlyList<CheckoutLineDto>>.NotFound("Cart was not found.");
            }

            var preview = cart.Items
                .Select(item => new CheckoutLineDto(item.ListingId, item.Quantity, item.UnitPriceAtAddition, item.Quantity * item.UnitPriceAtAddition))
                .ToArray();

            return Result<IReadOnlyList<CheckoutLineDto>>.Success(preview);
        }, "Unable to get checkout preview.");
    }

    public async Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.ShippingAddressId == Guid.Empty || string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                return Result<CheckoutResponse>.ValidationFailure("Shipping address and order number are required.");
            }

            var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            if (cart is null || cart.Items.Count == 0)
            {
                return Result<CheckoutResponse>.ValidationFailure("Cart was not found or contained no items.");
            }

            var subtotal = cart.Items.Sum(item => item.Quantity * item.UnitPriceAtAddition);
            var customerId = request.UserId ?? Guid.Empty;
            var order = new Order
            {
                CustomerId = customerId,
                ShippingAddressId = request.ShippingAddressId,
                OrderNumber = request.OrderNumber.Trim(),
                OrderStatus = OrderStatus.Pending,
                OrderPurchaseTimestampUtc = _dateTimeProvider.UtcNow,
                SubtotalAmount = subtotal,
                FreightAmount = 0m,
                TotalAmount = subtotal,
                PlacedFromCartId = cart.Id
            };

            var orderItems = cart.Items.Select((item, index) => new OrderItem
            {
                OrderId = order.Id,
                OrderItemId = index + 1,
                ListingId = item.ListingId,
                ProductId = Guid.Empty,
                SellerId = Guid.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPriceAtAddition,
                FreightValue = 0m
            }).ToArray();

            foreach (var orderItem in orderItems)
            {
                order.Items.Add(orderItem);
            }

            await _orderRepository.AddAsync(order, cancellationToken);

            var createdPayments = new List<PaymentDto>();
            var paymentSequential = 1;
            foreach (var paymentRequest in request.Payments)
            {
                var payment = new OrderPayment
                {
                    OrderId = order.Id,
                    PaymentSequential = paymentSequential++,
                    CurrencyId = paymentRequest.CurrencyId,
                    PaymentType = paymentRequest.PaymentType,
                    PaymentInstallments = paymentRequest.PaymentInstallments,
                    PaymentValue = paymentRequest.PaymentValue,
                    ExternalPaymentReference = paymentRequest.ExternalPaymentReference,
                    PaymentStatus = PaymentStatus.Pending
                };

                await _paymentRepository.AddAsync(payment, cancellationToken);
                order.Payments.Add(payment);
                createdPayments.Add(payment.ToDto());
            }

            cart.Status = CartStatus.Converted;
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            return Result<CheckoutResponse>.Success(new CheckoutResponse(order.ToDto(), cart.ToDto(), createdPayments, order.TotalAmount));
        }, "Unable to complete checkout.");
    }

    private async Task<ShoppingCart?> ResolveCartAsync(Guid? cartId, Guid? userId, Guid? sessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (cartId.HasValue && cartId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetByIdAsync(cartId.Value, cancellationToken);
        }
        else if (userId.HasValue && userId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetActiveByUserIdAsync(userId.Value, cancellationToken);
        }
        else if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            cart = await _cartRepository.GetActiveBySessionIdAsync(sessionId.Value, cancellationToken);
        }

        return cart is not null && cart.ExpiresAtUtc <= _dateTimeProvider.UtcNow
            ? null
            : cart;
    }
}
