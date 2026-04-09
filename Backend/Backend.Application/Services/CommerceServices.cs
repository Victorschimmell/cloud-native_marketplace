using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<OrderDto>.Failure("Order id is required.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return order is null
            ? Result<OrderDto>.Failure("Order was not found.")
            : Result<OrderDto>.Success(order.ToDto());
    }

    public async Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            return Result<PagedResult<OrderDto>>.Failure("Customer id is required.");
        }

        if (request.Page <= 0 || request.PageSize <= 0)
        {
            return Result<PagedResult<OrderDto>>.Failure("Page and page size must be greater than zero.");
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, request.Page, request.PageSize, cancellationToken);
        var items = orders.Select(static order => order.ToDto()).ToArray();
        return Result<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>(items, request.Page, request.PageSize, items.Length));
    }

    public async Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<OrderItemDto>>.Failure("Order id is required.");
        }

        var items = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<OrderItemDto>>.Success(items.Select(static item => item.ToDto()).ToArray());
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            return Result<OrderDto>.Failure("Order id is required.");
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order was not found.");
        }

        order.OrderStatus = request.Status;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<OrderDto>.Success(order.ToDto());
    }

    public async Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            return Result<OrderDto>.Failure("Order id is required.");
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order was not found.");
        }

        order.OrderStatus = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<OrderDto>.Success(order.ToDto());
    }
}

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<PaymentDto>>.Failure("Order id is required.");
        }

        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<PaymentDto>>.Success(payments.Select(static payment => payment.ToDto()).ToArray());
    }

    public async Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty || request.CurrencyId == Guid.Empty || request.PaymentValue <= 0)
        {
            return Result<PaymentDto>.Failure("Order id, currency id, and payment value are required.");
        }

        if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
        {
            return Result<PaymentDto>.Failure("Order was not found.");
        }

        var existingPayments = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        var payment = new OrderPayment
        {
            OrderId = request.OrderId,
            PaymentSequential = existingPayments.Count + 1,
            CurrencyId = request.CurrencyId,
            PaymentType = request.PaymentType,
            PaymentInstallments = request.PaymentInstallments,
            PaymentValue = request.PaymentValue,
            ExternalPaymentReference = request.ExternalPaymentReference,
            PaymentStatus = PaymentStatus.Pending,
            PaidAtUtc = _dateTimeProvider.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentDto>.Success(payment.ToDto());
    }
}

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(reviewRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<ReviewDto>>.Failure("Order id is required.");
        }

        var reviews = await _reviewRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(static review => review.ToDto()).ToArray());
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result<IReadOnlyList<ReviewDto>>.Failure("Product id is required.");
        }

        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        return Result<IReadOnlyList<ReviewDto>>.Success(reviews.Select(static review => review.ToDto()).ToArray());
    }

    public async Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty || request.ReviewScore < 1 || request.ReviewScore > 5)
        {
            return Result<ReviewDto>.Failure("Order id is required and review score must be between 1 and 5.");
        }

        if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
        {
            return Result<ReviewDto>.Failure("Order was not found.");
        }

        var review = new OrderReview
        {
            OrderId = request.OrderId,
            ReviewScore = request.ReviewScore,
            ReviewCommentTitle = request.ReviewCommentTitle,
            ReviewCommentMessage = request.ReviewCommentMessage,
            ReviewCreationDateUtc = _dateTimeProvider.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ReviewDto>.Success(review.ToDto());
    }
}

public sealed class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductListingRepository _productListingRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(ICartRepository cartRepository, IProductListingRepository productListingRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _cartRepository = cartRepository;
        _productListingRepository = productListingRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        return cart is null
            ? Result<CartDto>.Failure("Cart was not found.")
            : Result<CartDto>.Success(cart.ToDto());
    }

    public async Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ListingId == Guid.Empty || request.Quantity <= 0)
        {
            return Result<CartDto>.Failure("Listing id is required and quantity must be greater than zero.");
        }

        var listing = await _productListingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing is null)
        {
            return Result<CartDto>.Failure("Listing was not found.");
        }

        var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken)
            ?? new ShoppingCart
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                Status = CartStatus.Active,
                ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(7)
            };

        var existingItem = cart.Items.FirstOrDefault(item => item.ListingId == request.ListingId);
        if (existingItem is null)
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ListingId = listing.Id,
                Quantity = request.Quantity,
                UnitPriceAtAddition = listing.ListingPrice,
                AddedAtUtc = _dateTimeProvider.UtcNow,
                UpdatedAtUtc = _dateTimeProvider.UtcNow
            });
        }
        else
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAtUtc = _dateTimeProvider.UtcNow;
        }

        if (await _cartRepository.GetByIdAsync(cart.Id, cancellationToken) is null)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CartDto>.Success(cart.ToDto());
    }

    public async Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ListingId == Guid.Empty)
        {
            return Result<CartDto>.Failure("Listing id is required.");
        }

        var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<CartDto>.Failure("Cart was not found.");
        }

        var item = cart.Items.FirstOrDefault(cartItem => cartItem.ListingId == request.ListingId);
        if (item is null)
        {
            return Result<CartDto>.Failure("Cart item was not found.");
        }

        cart.Items.Remove(item);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CartDto>.Success(cart.ToDto());
    }

    private async Task<ShoppingCart?> ResolveCartAsync(Guid? cartId, Guid? userId, Guid? sessionId, CancellationToken cancellationToken)
    {
        if (cartId.HasValue && cartId.Value != Guid.Empty)
        {
            return await _cartRepository.GetByIdAsync(cartId.Value, cancellationToken);
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            return await _cartRepository.GetActiveByUserIdAsync(userId.Value, cancellationToken);
        }

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            return await _cartRepository.GetActiveBySessionIdAsync(sessionId.Value, cancellationToken);
        }

        return null;
    }
}

public sealed class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(ICartRepository cartRepository, IOrderRepository orderRepository, IPaymentRepository paymentRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<IReadOnlyList<CheckoutLineDto>>.Failure("Cart was not found.");
        }

        var preview = cart.Items
            .Select(item => new CheckoutLineDto(item.ListingId, item.Quantity, item.UnitPriceAtAddition, item.Quantity * item.UnitPriceAtAddition))
            .ToArray();

        return Result<IReadOnlyList<CheckoutLineDto>>.Success(preview);
    }

    public async Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ShippingAddressId == Guid.Empty || string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            return Result<CheckoutResponse>.Failure("Shipping address and order number are required.");
        }

        var cart = await ResolveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            return Result<CheckoutResponse>.Failure("Cart was not found or contained no items.");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CheckoutResponse>.Success(new CheckoutResponse(order.ToDto(), cart.ToDto(), createdPayments, order.TotalAmount));
    }

    private async Task<ShoppingCart?> ResolveCartAsync(Guid? cartId, Guid? userId, Guid? sessionId, CancellationToken cancellationToken)
    {
        if (cartId.HasValue && cartId.Value != Guid.Empty)
        {
            return await _cartRepository.GetByIdAsync(cartId.Value, cancellationToken);
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            return await _cartRepository.GetActiveByUserIdAsync(userId.Value, cancellationToken);
        }

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            return await _cartRepository.GetActiveBySessionIdAsync(sessionId.Value, cancellationToken);
        }

        return null;
    }
}

public sealed class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentService(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(shipmentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ShipmentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result<IReadOnlyList<ShipmentDto>>.Failure("Order id is required.");
        }

        var shipments = await _shipmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return Result<IReadOnlyList<ShipmentDto>>.Success(shipments.Select(static shipment => shipment.ToDto()).ToArray());
    }

    public async Task<Result<ShipmentDto>> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty || request.SellerId == Guid.Empty || string.IsNullOrWhiteSpace(request.CarrierName) || string.IsNullOrWhiteSpace(request.TrackingNumber))
        {
            return Result<ShipmentDto>.Failure("Order, seller, carrier, and tracking number are required.");
        }

        if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
        {
            return Result<ShipmentDto>.Failure("Order was not found.");
        }

        var shipment = new Shipment
        {
            OrderId = request.OrderId,
            SellerId = request.SellerId,
            CarrierName = request.CarrierName.Trim(),
            TrackingNumber = request.TrackingNumber.Trim(),
            ShipmentStatus = request.ShipmentStatus,
            ShippedAtUtc = _dateTimeProvider.UtcNow
        };

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ShipmentDto>.Success(shipment.ToDto());
    }

    public async Task<Result<ShipmentDto>> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ShipmentId == Guid.Empty)
        {
            return Result<ShipmentDto>.Failure("Shipment id is required.");
        }

        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            return Result<ShipmentDto>.Failure("Shipment was not found.");
        }

        shipment.ShipmentStatus = request.ShipmentStatus;
        if (request.ShipmentStatus == ShipmentStatus.Delivered)
        {
            shipment.DeliveredAtUtc = _dateTimeProvider.UtcNow;
        }

        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ShipmentDto>.Success(shipment.ToDto());
    }
}
