using System.Text.Json;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.Catalog;
using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class CheckoutService : ICheckoutService
{
    private const decimal FlatFreightAmount = 100m;

    private readonly ICartRepository _cartRepository;
    private readonly IProductListingRepository _productListingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ICustomerRepository _customerRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IPaymentService _paymentService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckoutObservability _checkoutObservability;

    public CheckoutService(
        ICartRepository cartRepository,
        IProductListingRepository productListingRepository,
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IOrderNumberGenerator orderNumberGenerator,
        ICustomerRepository customerRepository,
        ISellerRepository sellerRepository,
        IAddressRepository addressRepository,
        IPaymentService paymentService,
        IDateTimeProvider dateTimeProvider,
        ICurrencyConversionService currencyConversionService,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ICheckoutObservability checkoutObservability)
    {
        ArgumentNullException.ThrowIfNull(cartRepository);
        ArgumentNullException.ThrowIfNull(productListingRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(orderItemRepository);
        ArgumentNullException.ThrowIfNull(orderNumberGenerator);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(addressRepository);
        ArgumentNullException.ThrowIfNull(paymentService);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(currencyConversionService);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(checkoutObservability);

        _cartRepository = cartRepository;
        _productListingRepository = productListingRepository;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderNumberGenerator = orderNumberGenerator;
        _customerRepository = customerRepository;
        _sellerRepository = sellerRepository;
        _addressRepository = addressRepository;
        _paymentService = paymentService;
        _dateTimeProvider = dateTimeProvider;
        _currencyConversionService = currencyConversionService;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _checkoutObservability = checkoutObservability;
    }

    public async Task<Result<CheckoutPreviewDto>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        if (await GetBuyerRestrictionAsync(request.UserId, cancellationToken) is { } restriction)
        {
            return Result<CheckoutPreviewDto>.Forbidden(restriction);
        }

        if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
        {
            return Result<CheckoutPreviewDto>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
        }

        var activeCart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
        if (activeCart is null)
        {
            return Result<CheckoutPreviewDto>.NotFound("Cart was not found for the provided identifiers.");
        }

        var cart = await GetCartWithProductDetailsAsync(activeCart.Id, request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
        {
            return Result<CheckoutPreviewDto>.NotFound("Cart was not found for the provided identifiers.");
        }

        if (!_currencyConversionService.TryGetPriceFromBaseConverter(displayCurrency, out var currencyCode, out var priceConverter))
        {
            return Result<CheckoutPreviewDto>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
        }

        var checkoutLines = cart.Items.Select(item => new CheckoutLineDto(
            item.ListingId,
            item.Listing?.ProductId ?? throw new InvalidOperationException("Cart item must include listing details."),
            item.Listing?.Product?.ProductName ?? throw new InvalidOperationException("Cart item must include listing product details."),
            item.Quantity,
            priceConverter(item.UnitPriceAtAddition),
            priceConverter(item.UnitPriceAtAddition * item.Quantity),
            currencyCode)).ToArray();

        var subtotalAmount = cart.Items.Sum(i => i.UnitPriceAtAddition * i.Quantity);
        var freightAmount = FlatFreightAmount;
        var totalAmount = subtotalAmount + freightAmount;
        var preview = new CheckoutPreviewDto(
            checkoutLines,
            priceConverter(subtotalAmount),
            priceConverter(freightAmount),
            priceConverter(totalAmount),
            currencyCode);

        return Result<CheckoutPreviewDto>.Success(preview);
    }

    public async Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, string displayCurrency, CancellationToken cancellationToken = default)
    {
        var checkoutStartedAt = _checkoutObservability.GetTimestamp();
        var context = ObservabilityContext(request.UserId, request.CartId, currencyCode: displayCurrency, paymentCount: request.Payments.Count);
        _checkoutObservability.Started(checkoutStartedAt, context);

        Result<CheckoutResponse> FailCheckout(Result<CheckoutResponse> result, string errorType, CheckoutObservabilityContext failureContext)
        {
            _checkoutObservability.Failed("Checkout.Process", errorType, checkoutStartedAt, failureContext);
            return result;
        }

        try
        {
            var stepStartedAt = _checkoutObservability.GetTimestamp();
            if (await GetBuyerRestrictionAsync(request.UserId, cancellationToken) is { } restriction)
            {
                _checkoutObservability.Failed(
                    "Checkout.CustomerValidated",
                    "BuyerRestricted",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.Forbidden(restriction),
                    "BuyerRestricted",
                    context);
            }

            stepStartedAt = _checkoutObservability.GetTimestamp();
            if (!_currencyConversionService.TryGetPriceFromBaseConverter(displayCurrency, out var currencyCode, out var priceConverter))
            {
                _checkoutObservability.Failed(
                    "Checkout.PaymentValidated",
                    "InvalidCurrency",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("Currency must be one of BRL, USD, or DKK."),
                    "InvalidCurrency",
                    context);
            }

            context = context with { CurrencyCode = currencyCode };

            stepStartedAt = _checkoutObservability.GetTimestamp();
            if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
            {
                _checkoutObservability.Failed(
                    "Checkout.CartLoaded",
                    "MissingCartIdentifier",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided."),
                    "MissingCartIdentifier",
                    context);
            }

            stepStartedAt = _checkoutObservability.GetTimestamp();
            var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            if (cart is null)
            {
                _checkoutObservability.Failed(
                    "Checkout.CartLoaded",
                    "CartNotFound",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.NotFound("Cart was not found for the provided identifiers."),
                    "CartNotFound",
                    context);
            }
            context = context with { CartId = cart.Id, ItemCount = cart.Items.Count, PaymentCount = request.Payments.Count };
            _checkoutObservability.Succeeded("Checkout.CartLoaded", stepStartedAt, context);

            stepStartedAt = _checkoutObservability.GetTimestamp();
            var customerUserId = request.UserId ?? cart.UserId;
            if (!customerUserId.HasValue)
            {
                _checkoutObservability.Failed(
                    "Checkout.CustomerValidated",
                    "UnauthenticatedCustomer",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.Unauthorized("Checkout requires an authenticated customer."),
                    "UnauthenticatedCustomer",
                    context);
            }

            context = context with { UserId = customerUserId };

            var customer = await _customerRepository.GetByUserIdAsync(customerUserId.Value, cancellationToken);
            if (customer is null)
            {
                _checkoutObservability.Failed(
                    "Checkout.CustomerValidated",
                    "CustomerProfileNotFound",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.NotFound("Customer profile was not found for the authenticated user."),
                    "CustomerProfileNotFound",
                    context);
            }
            _checkoutObservability.Succeeded("Checkout.CustomerValidated", stepStartedAt, context);

            stepStartedAt = _checkoutObservability.GetTimestamp();
            if (!TryCreateShippingAddress(request.ShippingAddress, out var shippingAddress, out var shippingAddressError))
            {
                _checkoutObservability.Failed(
                    "Checkout.ShippingAddressValidated",
                    "InvalidShippingAddress",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure(shippingAddressError),
                    "InvalidShippingAddress",
                    context);
            }
            _checkoutObservability.Succeeded("Checkout.ShippingAddressValidated", stepStartedAt, context);

            stepStartedAt = _checkoutObservability.GetTimestamp();
            if (request.Payments.Count == 0)
            {
                _checkoutObservability.Failed(
                    "Checkout.PaymentValidated",
                    "MissingPayment",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("At least one payment is required."),
                    "MissingPayment",
                    context);
            }

            if (request.Payments.Any(payment => payment.PaymentType != PaymentType.CreditCard))
            {
                _checkoutObservability.Failed(
                    "Checkout.PaymentValidated",
                    "UnsupportedPaymentType",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("Only credit card payments are supported at checkout."),
                    "UnsupportedPaymentType",
                    context);
            }

            var now = _dateTimeProvider.UtcNow;
            var orderNumber = await _orderNumberGenerator.GenerateOrderNumberAsync(cancellationToken);
            context = context with { OrderNumber = orderNumber };

            stepStartedAt = _checkoutObservability.GetTimestamp();
            if (cart.Items.Count == 0)
            {
                _checkoutObservability.Failed(
                    "Checkout.InventoryValidated",
                    "EmptyCart",
                    stepStartedAt,
                    context);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("Checkout requires at least one cart item."),
                    "EmptyCart",
                    context);
            }

            var listingsById = new Dictionary<Guid, ProductListing>();

            // check stock availability for each cart item, if any of the items is not available in the requested quantity, return failure result
            foreach (var item in cart.Items)
            {
                var listing = await _productListingRepository.GetByIdAsync(item.ListingId, cancellationToken);
                if (listing is null || listing.IsDeleted || listing.VisibilityStatus != ListingVisibilityStatus.Published)
                {
                    _checkoutObservability.Failed(
                        "Checkout.InventoryValidated",
                        "ListingNotFound",
                        stepStartedAt,
                        context);
                    return FailCheckout(
                        Result<CheckoutResponse>.NotFound($"Product listing with id {item.ListingId} was not found."),
                        "ListingNotFound",
                        context);
                }

                if (listing.InventoryQuantity < item.Quantity)
                {
                    _checkoutObservability.InventoryFailed(
                        stepStartedAt,
                        context,
                        item.ListingId,
                        item.Quantity,
                        listing.InventoryQuantity);
                    return FailCheckout(
                        Result<CheckoutResponse>.ValidationFailure($"Product listing with id {item.ListingId} does not have enough stock. Available quantity: {listing.InventoryQuantity}, requested quantity: {item.Quantity}."),
                        "InsufficientInventory",
                        context);
                }

                if (listing.Seller?.VerificationStatus != VerificationStatus.Verified)
                {
                    _checkoutObservability.Failed(
                        "Checkout.InventoryValidated",
                        "SellerNotVerified",
                        stepStartedAt,
                        context);
                    return FailCheckout(
                        Result<CheckoutResponse>.ValidationFailure($"Seller of product listing {item.ListingId} is not verified. Products from unverified sellers cannot be purchased."),
                        "SellerNotVerified",
                        context);
                }

                if (listing.Seller.UserAccount?.IsBlocked == true)
                {
                    _checkoutObservability.Failed(
                        "Checkout.InventoryValidated",
                        "SellerNotActive",
                        stepStartedAt,
                        context);
                    return FailCheckout(
                        Result<CheckoutResponse>.ValidationFailure($"Seller of product listing {item.ListingId} is not active. Products from inactive sellers cannot be purchased."),
                        "SellerNotActive",
                        context);
                }

                listingsById[item.ListingId] = listing;
            }
            _checkoutObservability.Succeeded("Checkout.InventoryValidated", stepStartedAt, context);

            // check payment amount is consistent with the checkout total, if not, return failure result
            stepStartedAt = _checkoutObservability.GetTimestamp();
            var subtotalAmount = cart.Items.Sum(i => i.UnitPriceAtAddition * i.Quantity);
            var freightAmount = FlatFreightAmount;
            var totalAmount = subtotalAmount + freightAmount;
            var expectedPaymentAmount = priceConverter(totalAmount);
            var requestedPaymentAmount = request.Payments.Sum(p => p.PaymentValue);
            context = context with { TotalAmount = totalAmount };
            if (requestedPaymentAmount != expectedPaymentAmount)
            {
                _checkoutObservability.PaymentAmountFailed(
                    stepStartedAt,
                    context,
                    requestedPaymentAmount,
                    expectedPaymentAmount);
                return FailCheckout(
                    Result<CheckoutResponse>.ValidationFailure("Payment amount in the request does not match the calculated checkout total amount."),
                    "PaymentAmountMismatch",
                    context);
            }
            _checkoutObservability.Succeeded("Checkout.PaymentValidated", stepStartedAt, context);

            Result<CheckoutResponse>? transactionFailure = null;
            CheckoutResponse? response = null;

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
                {
                    // 1. Create Order
                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    await _addressRepository.AddAsync(shippingAddress, transactionCancellationToken);
                    if (request.SaveShippingAddressAsDefault)
                    {
                        customer.DefaultAddressId = shippingAddress.Id;
                        await _customerRepository.UpdateAsync(customer, transactionCancellationToken);
                    }

                    var customerId = customer.Id;
                    var order = new Order
                    {
                        CustomerId = customerId,
                        ShippingAddressId = shippingAddress.Id,
                        OrderStatus = OrderStatus.Pending,
                        OrderPurchaseTimestampUtc = now,
                        SubtotalAmount = subtotalAmount,
                        FreightAmount = freightAmount,
                        TotalAmount = totalAmount,
                        PlacedFromCartId = cart.Id,
                        OrderNumber = orderNumber
                    };
                    await _orderRepository.AddAsync(order, transactionCancellationToken);
                    context = context with { OrderId = order.Id };
                    _checkoutObservability.Succeeded("Checkout.OrderCreated", stepStartedAt, context);

                    // 2. process payments
                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    var payments = new List<PaymentDto>();
                    foreach (var paymentRequest in request.Payments)
                    {
                        var paymentRequestWithOrderId = new RecordPaymentRequest(order.Id, paymentRequest);
                        var paymentResult = await _paymentService.RecordCheckoutPaymentAsync(paymentRequestWithOrderId, transactionCancellationToken);
                        if (!paymentResult.IsSuccess)
                        {
                            _checkoutObservability.Failed(
                                "Checkout.PaymentRecorded",
                                "PaymentProcessingFailed",
                                stepStartedAt,
                                context);

                            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                                ActionType: AuditActionType.Created,
                                TargetEntityType: nameof(Order),
                                TargetEntityId: order.Id.ToString(),
                                Outcome: AuditOutcome.Failed,
                                Details: $"Payment processing failed: {paymentResult.Error}"
                            ), transactionCancellationToken);
                            transactionFailure = FailCheckout(
                                Result<CheckoutResponse>.Failure("Payment processing failed: " + paymentResult.Error),
                                "PaymentProcessingFailed",
                                context);
                            throw new CheckoutTransactionFailedException();
                        }

                        if (paymentResult.Value is null)
                        {
                            _checkoutObservability.Failed(
                                "Checkout.PaymentRecorded",
                                "PaymentProcessingFailed",
                                stepStartedAt,
                                context);
                            transactionFailure = FailCheckout(
                                Result<CheckoutResponse>.Failure("Payment processing failed: payment result was null."),
                                "PaymentProcessingFailed",
                                context);
                            throw new CheckoutTransactionFailedException();
                        }
                        payments.Add(paymentResult.Value);
                    }
                    context = context with { PaymentCount = payments.Count };
                    _checkoutObservability.Succeeded("Checkout.PaymentRecorded", stepStartedAt, context);

                    // 3. Create order items, decrement stock, and update cart/order status
                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    await CreateOrderItemsAndDeductStockAsync(order, cart, listingsById, freightAmount, transactionCancellationToken);
                    _checkoutObservability.Succeeded("Checkout.OrderItemsCreatedAndStockDeducted", stepStartedAt, context);

                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    cart.Status = CartStatus.Converted;
                    await _cartRepository.UpdateAsync(cart, transactionCancellationToken);
                    _checkoutObservability.Succeeded("Checkout.CartConverted", stepStartedAt, context);

                    // 4. Save all changes
                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
                    _checkoutObservability.Succeeded("Checkout.ChangesSaved", stepStartedAt, context);

                    stepStartedAt = _checkoutObservability.GetTimestamp();
                    await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                        ActionType: AuditActionType.Created,
                        TargetEntityType: nameof(Order),
                        TargetEntityId: order.Id.ToString(),
                        Outcome: AuditOutcome.Succeeded,
                        Details: JsonSerializer.Serialize(new
                        {
                            order.OrderNumber,
                            customerId,
                            totalAmount,
                            payments.Count
                        })
                    ), transactionCancellationToken);
                    _checkoutObservability.Succeeded("Checkout.AuditLogWritten", stepStartedAt, context);

                    // 5. Build response from saved state
                    var savedOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id, transactionCancellationToken) ??
                        throw new InvalidOperationException("Order must exist after checkout.");

                    var savedCart = await _cartRepository.GetByIdWithProductDetailsAsync(cart.Id, transactionCancellationToken) ??
                        throw new InvalidOperationException("Cart must exist after checkout.");

                    response = new CheckoutResponse(
                        savedOrder.ToOrderDto(customer.UserId, currencyCode, priceConverter),
                        savedCart.ToCartDto(currencyCode, priceConverter),
                        payments,
                        priceConverter(order.TotalAmount),
                        currencyCode);
                }, cancellationToken);
            }
            catch (CheckoutTransactionFailedException)
            {
                return transactionFailure ?? Result<CheckoutResponse>.Failure("Checkout failed.");
            }

            _checkoutObservability.Succeeded("Checkout.Process", checkoutStartedAt, context);
            return Result<CheckoutResponse>.Success(response ?? throw new InvalidOperationException("Checkout response must be created before returning success."));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _checkoutObservability.Unexpected(exception, checkoutStartedAt, context);
            throw;
        }
    }

    private static CheckoutObservabilityContext ObservabilityContext(
        Guid? userId,
        Guid? cartId,
        Guid? orderId = null,
        string? orderNumber = null,
        string? currencyCode = null,
        int? itemCount = null,
        int? paymentCount = null,
        decimal? totalAmount = null) =>
        new(userId, cartId, orderId, orderNumber, currencyCode, itemCount, paymentCount, totalAmount);

    private async Task CreateOrderItemsAndDeductStockAsync(
        Order order,
        ShoppingCart cart,
        IReadOnlyDictionary<Guid, ProductListing> listingsById,
        decimal freightAmount,
        CancellationToken cancellationToken)
    {
        var freightAllocations = AllocateFreightAcrossCartItems(cart.Items.Count, freightAmount);
        var orderItemId = 1;

        foreach (var item in cart.Items)
        {
            var listing = listingsById[item.ListingId];
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                OrderItemId = orderItemId,
                ListingId = listing.Id,
                ProductId = listing.ProductId,
                SellerId = listing.SellerId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPriceAtAddition,
                FreightValue = freightAllocations[orderItemId - 1]
            };

            listing.InventoryQuantity -= item.Quantity;
            order.Items.Add(orderItem);

            await _orderItemRepository.AddAsync(orderItem, cancellationToken);
            await _productListingRepository.UpdateAsync(listing, cancellationToken);
            orderItemId += 1;
        }
    }

    private static decimal[] AllocateFreightAcrossCartItems(int itemCount, decimal freightAmount)
    {
        if (itemCount <= 0)
        {
            return [];
        }

        var allocations = new decimal[itemCount];
        var baseAllocation = decimal.Round(freightAmount / itemCount, 4, MidpointRounding.AwayFromZero);

        for (var i = 0; i < allocations.Length; i += 1)
        {
            allocations[i] = baseAllocation;
        }

        allocations[^1] += freightAmount - allocations.Sum();
        return allocations;
    }

    private static bool TryCreateShippingAddress(
        CheckoutShippingAddressDto request,
        out Address address,
        out string error)
    {
        address = null!;
        error = string.Empty;

        var postalCode = Normalize(request.PostalCode);
        var city = Normalize(request.City);
        var state = Normalize(request.State);
        var addressLine1 = Normalize(request.AddressLine1);
        var addressLine2 = NormalizeOptional(request.AddressLine2);
        var countryCode = Normalize(request.CountryCode).ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(addressLine1))
        {
            error = "Shipping address line 1 is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            error = "Shipping city is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            error = "Shipping state is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            error = "Shipping postal code is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            error = "Shipping country code is required.";
            return false;
        }

        address = new Address
        {
            PostalCode = postalCode,
            City = city,
            State = state,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            CountryCode = countryCode
        };
        return true;
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private async Task<string?> GetBuyerRestrictionAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var seller = await _sellerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (seller is not null)
        {
            return "Seller accounts cannot check out or buy products.";
        }

        var customer = await _customerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (customer is null)
        {
            return "Only customer accounts can check out.";
        }

        return null;
    }

    private async Task<ShoppingCart?> GetCartAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdAsync(CartId.Value, cancellationToken);
            if (cart is not null && !CartAccessPolicy.CanAccess(cart, UserId, SessionId))
            {
                return null;
            }
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

    private async Task<ShoppingCart?> GetCartWithProductDetailsAsync(Guid? CartId, Guid? UserId, Guid? SessionId, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (CartId.HasValue)
        {
            cart = await _cartRepository.GetByIdWithProductDetailsAsync(CartId.Value, cancellationToken);
            if (cart is not null && !CartAccessPolicy.CanAccess(cart, UserId, SessionId))
            {
                return null;
            }
        }

        if (cart is null && UserId.HasValue)
        {
            cart = await _cartRepository.GetActiveByUserIdWithProductDetailsAsync(UserId.Value, cancellationToken);
        }

        if (cart is null && SessionId.HasValue)
        {
            cart = await _cartRepository.GetActiveBySessionIdWithProductDetailsAsync(SessionId.Value, cancellationToken);
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

    private sealed class CheckoutTransactionFailedException : Exception
    {
    }
}
