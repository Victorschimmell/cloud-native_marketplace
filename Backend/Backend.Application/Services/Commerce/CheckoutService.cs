using System.Diagnostics;
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
using Microsoft.Extensions.Logging;

namespace Backend.Application.Services;

public sealed class CheckoutService : ICheckoutService
{
    private const string ComponentName = "CheckoutService";

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
    private readonly ILogger<CheckoutService> _logger;

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
        ILogger<CheckoutService> logger)
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
        ArgumentNullException.ThrowIfNull(logger);

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
        _logger = logger;
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

        if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
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
        var freightAmount = 100m;
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
        var checkoutStartedAt = Stopwatch.GetTimestamp();
        LogCheckoutStep(
            "Checkout.Started",
            "Started",
            checkoutStartedAt,
            request.UserId,
            request.CartId,
            orderId: null,
            orderNumber: null,
            displayCurrency,
            itemCount: null,
            request.Payments.Count);

        try
        {
            var stepStartedAt = Stopwatch.GetTimestamp();
            if (await GetBuyerRestrictionAsync(request.UserId, cancellationToken) is { } restriction)
            {
                LogCheckoutFailure(
                    "Checkout.CustomerValidated",
                    "BuyerRestricted",
                    stepStartedAt,
                    request.UserId,
                    request.CartId,
                    orderId: null,
                    orderNumber: null,
                    displayCurrency);
                return Result<CheckoutResponse>.Forbidden(restriction);
            }

            if (!_currencyConversionService.TryGetPriceConverter(displayCurrency, out var currencyCode, out var priceConverter))
            {
                LogCheckoutFailure(
                    "Checkout.PaymentValidated",
                    "InvalidCurrency",
                    checkoutStartedAt,
                    request.UserId,
                    request.CartId,
                    orderId: null,
                    orderNumber: null,
                    displayCurrency);
                return Result<CheckoutResponse>.ValidationFailure("Currency must be one of BRL, USD, or DKK.");
            }

            if (!request.CartId.HasValue && !request.UserId.HasValue && !request.SessionId.HasValue)
            {
                LogCheckoutFailure(
                    "Checkout.CartLoaded",
                    "MissingCartIdentifier",
                    checkoutStartedAt,
                    request.UserId,
                    request.CartId,
                    orderId: null,
                    orderNumber: null,
                    currencyCode);
                return Result<CheckoutResponse>.ValidationFailure("At least one of CartId, UserId, or SessionId must be provided.");
            }

            stepStartedAt = Stopwatch.GetTimestamp();
            var cart = await GetActiveCartAsync(request.CartId, request.UserId, request.SessionId, cancellationToken);
            if (cart is null)
            {
                LogCheckoutFailure(
                    "Checkout.CartLoaded",
                    "CartNotFound",
                    stepStartedAt,
                    request.UserId,
                    request.CartId,
                    orderId: null,
                    orderNumber: null,
                    currencyCode);
                return Result<CheckoutResponse>.NotFound("Cart was not found for the provided identifiers.");
            }
            LogCheckoutStep("Checkout.CartLoaded", "Succeeded", stepStartedAt, request.UserId, cart.Id, orderId: null, orderNumber: null, currencyCode, cart.Items.Count, request.Payments.Count);

            stepStartedAt = Stopwatch.GetTimestamp();
            var customerUserId = request.UserId ?? cart.UserId;
            if (!customerUserId.HasValue)
            {
                LogCheckoutFailure(
                    "Checkout.CustomerValidated",
                    "UnauthenticatedCustomer",
                    stepStartedAt,
                    request.UserId,
                    cart.Id,
                    orderId: null,
                    orderNumber: null,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.Unauthorized("Checkout requires an authenticated customer.");
            }

            var customer = await _customerRepository.GetByUserIdAsync(customerUserId.Value, cancellationToken);
            if (customer is null)
            {
                LogCheckoutFailure(
                    "Checkout.CustomerValidated",
                    "CustomerProfileNotFound",
                    stepStartedAt,
                    customerUserId,
                    cart.Id,
                    orderId: null,
                    orderNumber: null,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.NotFound("Customer profile was not found for the authenticated user.");
            }
            LogCheckoutStep("Checkout.CustomerValidated", "Succeeded", stepStartedAt, customerUserId, cart.Id, orderId: null, orderNumber: null, currencyCode, cart.Items.Count, request.Payments.Count);

            stepStartedAt = Stopwatch.GetTimestamp();
            if (!TryCreateShippingAddress(request.ShippingAddress, out var shippingAddress, out var shippingAddressError))
            {
                LogCheckoutFailure(
                    "Checkout.ShippingAddressValidated",
                    "InvalidShippingAddress",
                    stepStartedAt,
                    customerUserId,
                    cart.Id,
                    orderId: null,
                    orderNumber: null,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.ValidationFailure(shippingAddressError);
            }
            LogCheckoutStep("Checkout.ShippingAddressValidated", "Succeeded", stepStartedAt, customerUserId, cart.Id, orderId: null, orderNumber: null, currencyCode, cart.Items.Count, request.Payments.Count);

            stepStartedAt = Stopwatch.GetTimestamp();
            if (request.Payments.Count == 0)
            {
                LogCheckoutFailure(
                    "Checkout.PaymentValidated",
                    "MissingPayment",
                    stepStartedAt,
                    customerUserId,
                    cart.Id,
                    orderId: null,
                    orderNumber: null,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.ValidationFailure("At least one payment is required.");
            }

            if (request.Payments.Any(payment => payment.PaymentType != PaymentType.CreditCard))
            {
                LogCheckoutFailure(
                    "Checkout.PaymentValidated",
                    "UnsupportedPaymentType",
                    stepStartedAt,
                    customerUserId,
                    cart.Id,
                    orderId: null,
                    orderNumber: null,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.ValidationFailure("Only credit card payments are supported at checkout.");
            }

            var now = _dateTimeProvider.UtcNow;
            var orderNumber = await _orderNumberGenerator.GenerateOrderNumberAsync(cancellationToken);

            if (cart.Items.Count == 0)
            {
                LogCheckoutFailure(
                    "Checkout.InventoryValidated",
                    "EmptyCart",
                    stepStartedAt,
                    customerUserId,
                    cart.Id,
                    orderId: null,
                    orderNumber,
                    currencyCode,
                    cart.Items.Count,
                    request.Payments.Count);
                return Result<CheckoutResponse>.ValidationFailure("Checkout requires at least one cart item.");
            }

            var listingsById = new Dictionary<Guid, ProductListing>();

            // check stock availability for each cart item, if any of the items is not available in the requested quantity, return failure result
            stepStartedAt = Stopwatch.GetTimestamp();
            foreach (var item in cart.Items)
            {
                var listing = await _productListingRepository.GetByIdAsync(item.ListingId, cancellationToken);
                if (listing is null || listing.IsDeleted || listing.VisibilityStatus != ListingVisibilityStatus.Published)
                {
                    LogCheckoutFailure(
                        "Checkout.InventoryValidated",
                        "ListingNotFound",
                        stepStartedAt,
                        customerUserId,
                        cart.Id,
                        orderId: null,
                        orderNumber,
                        currencyCode,
                        cart.Items.Count,
                        request.Payments.Count,
                        listingId: item.ListingId);
                    return Result<CheckoutResponse>.NotFound($"Product listing with id {item.ListingId} was not found.");
                }

                if (listing.InventoryQuantity < item.Quantity)
                {
                    LogInventoryFailure(stepStartedAt, customerUserId, cart.Id, orderNumber, currencyCode, cart.Items.Count, request.Payments.Count, item.ListingId, item.Quantity, listing.InventoryQuantity);
                    return Result<CheckoutResponse>.ValidationFailure($"Product listing with id {item.ListingId} does not have enough stock. Available quantity: {listing.InventoryQuantity}, requested quantity: {item.Quantity}.");
                }

                listingsById[item.ListingId] = listing;
            }
            LogCheckoutStep("Checkout.InventoryValidated", "Succeeded", stepStartedAt, customerUserId, cart.Id, orderId: null, orderNumber, currencyCode, cart.Items.Count, request.Payments.Count);

            // check payment amount is consistent with the checkout total, if not, return failure result
            stepStartedAt = Stopwatch.GetTimestamp();
            var subtotalAmount = cart.Items.Sum(i => i.UnitPriceAtAddition * i.Quantity);
            var freightAmount = 100m;  // TODO: Implement proper freight calculation, currently using a fixed amount
            var totalAmount = subtotalAmount + freightAmount;
            var expectedPaymentAmount = priceConverter(totalAmount);
            var requestedPaymentAmount = request.Payments.Sum(p => p.PaymentValue);
            if (requestedPaymentAmount != expectedPaymentAmount)
            {
                LogPaymentAmountFailure(stepStartedAt, customerUserId, cart.Id, orderNumber, currencyCode, cart.Items.Count, request.Payments.Count, totalAmount, requestedPaymentAmount, expectedPaymentAmount);
                return Result<CheckoutResponse>.ValidationFailure("Payment amount in the request does not match the calculated checkout total amount.");
            }
            LogCheckoutStep("Checkout.PaymentValidated", "Succeeded", stepStartedAt, customerUserId, cart.Id, orderId: null, orderNumber, currencyCode, cart.Items.Count, request.Payments.Count, totalAmount);

            // 1. Create Order
            stepStartedAt = Stopwatch.GetTimestamp();
            await _addressRepository.AddAsync(shippingAddress, cancellationToken);
            if (request.SaveShippingAddressAsDefault)
            {
                customer.DefaultAddressId = shippingAddress.Id;
                await _customerRepository.UpdateAsync(customer, cancellationToken);
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
            await _orderRepository.AddAsync(order, cancellationToken);
            LogCheckoutStep("Checkout.OrderCreated", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, request.Payments.Count, totalAmount);

            // 2. process payments
            stepStartedAt = Stopwatch.GetTimestamp();
            var payments = new List<PaymentDto>();
            foreach (var paymentRequest in request.Payments)
            {
                var paymentRequestWithOrderId = new RecordPaymentRequest(order.Id, paymentRequest);
                // NOTE: Currently never fails
                var paymentResult = await _paymentService.RecordCheckoutPaymentAsync(paymentRequestWithOrderId, cancellationToken);
                if (!paymentResult.IsSuccess)
                {
                    LogCheckoutFailure(
                        "Checkout.PaymentRecorded",
                        "PaymentProcessingFailed",
                        stepStartedAt,
                        customerUserId,
                        cart.Id,
                        order.Id,
                        orderNumber,
                        currencyCode,
                        cart.Items.Count,
                        request.Payments.Count,
                        totalAmount);

                    // TODO: Implement rollback mechanism to undo the created order in case of payment failure, currently always success
                    // await _unitOfWork.RollbackAsync(cancellationToken);
                    await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                        ActionType: AuditActionType.Created,
                        TargetEntityType: nameof(Order),
                        TargetEntityId: order.Id.ToString(),
                        Outcome: AuditOutcome.Failed,
                        Details: $"Payment processing failed: {paymentResult.Error}"
                    ), cancellationToken);
                    return Result<CheckoutResponse>.Failure("Payment processing failed: " + paymentResult.Error);
                }

                if (paymentResult.Value is null)
                {
                    LogCheckoutFailure(
                        "Checkout.PaymentRecorded",
                        "PaymentProcessingFailed",
                        stepStartedAt,
                        customerUserId,
                        cart.Id,
                        order.Id,
                        orderNumber,
                        currencyCode,
                        cart.Items.Count,
                        request.Payments.Count,
                        totalAmount);
                    return Result<CheckoutResponse>.Failure("Payment processing failed: payment result was null.");
                }
                payments.Add(paymentResult.Value);
            }
            LogCheckoutStep("Checkout.PaymentRecorded", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);

            // 3. Create order items, decrement stock, and update cart/order status
            stepStartedAt = Stopwatch.GetTimestamp();
            await CreateOrderItemsAndDeductStockAsync(order, cart, listingsById, freightAmount, cancellationToken);
            LogCheckoutStep("Checkout.OrderItemsCreated", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);
            LogCheckoutStep("Checkout.StockDeducted", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);

            stepStartedAt = Stopwatch.GetTimestamp();
            cart.Status = CartStatus.Converted;
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            LogCheckoutStep("Checkout.CartConverted", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);

            // 4. Save all changes
            stepStartedAt = Stopwatch.GetTimestamp();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            LogCheckoutStep("Checkout.ChangesSaved", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);

            stepStartedAt = Stopwatch.GetTimestamp();
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
            ), cancellationToken);
            LogCheckoutStep("Checkout.AuditLogWritten", "Succeeded", stepStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);

            // 5. Return response
            var savedOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id, cancellationToken) ??
                throw new InvalidOperationException("Order must exist after checkout.");

            var savedCart = await _cartRepository.GetByIdWithProductDetailsAsync(cart.Id, cancellationToken) ??
                throw new InvalidOperationException("Cart must exist after checkout.");

            var response = new CheckoutResponse(
                savedOrder.ToOrderDto(customer.UserId, currencyCode, priceConverter),
                savedCart.ToCartDto(currencyCode, priceConverter),
                payments,
                priceConverter(order.TotalAmount),
                currencyCode);
            LogCheckoutStep("Checkout.Completed", "Succeeded", checkoutStartedAt, customerUserId, cart.Id, order.Id, orderNumber, currencyCode, cart.Items.Count, payments.Count, totalAmount);
            return Result<CheckoutResponse>.Success(response);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} CurrencyCode={CurrencyCode}",
                "Checkout.Failed",
                "Failed",
                ComponentName,
                ElapsedMilliseconds(checkoutStartedAt),
                "UnexpectedException",
                request.CartId,
                request.UserId,
                displayCurrency);
            throw;
        }
    }

    private void LogCheckoutStep(
        string operation,
        string outcome,
        long startedAt,
        Guid? userId,
        Guid? cartId,
        Guid? orderId,
        string? orderNumber,
        string currencyCode,
        int? itemCount = null,
        int? paymentCount = null,
        decimal? totalAmount = null)
    {
        _logger.LogInformation(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. CartId={CartId} UserId={UserId} OrderId={OrderId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount}",
            operation,
            outcome,
            ComponentName,
            ElapsedMilliseconds(startedAt),
            cartId,
            userId,
            orderId,
            orderNumber,
            currencyCode,
            itemCount,
            paymentCount,
            totalAmount);
    }

    private void LogCheckoutFailure(
        string operation,
        string errorType,
        long startedAt,
        Guid? userId,
        Guid? cartId,
        Guid? orderId,
        string? orderNumber,
        string currencyCode,
        int? itemCount = null,
        int? paymentCount = null,
        decimal? totalAmount = null,
        Guid? listingId = null)
    {
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderId={OrderId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount} ListingId={ListingId}",
            operation,
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            errorType,
            cartId,
            userId,
            orderId,
            orderNumber,
            currencyCode,
            itemCount,
            paymentCount,
            totalAmount,
            listingId);
    }

    private void LogInventoryFailure(
        long startedAt,
        Guid? userId,
        Guid cartId,
        string orderNumber,
        string currencyCode,
        int itemCount,
        int paymentCount,
        Guid listingId,
        int requestedQuantity,
        int availableQuantity)
    {
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} ListingId={ListingId} RequestedQuantity={RequestedQuantity} AvailableQuantity={AvailableQuantity}",
            "Checkout.InventoryValidated",
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            "InsufficientInventory",
            cartId,
            userId,
            orderNumber,
            currencyCode,
            itemCount,
            paymentCount,
            listingId,
            requestedQuantity,
            availableQuantity);
    }

    private void LogPaymentAmountFailure(
        long startedAt,
        Guid? userId,
        Guid cartId,
        string orderNumber,
        string currencyCode,
        int itemCount,
        int paymentCount,
        decimal totalAmount,
        decimal requestedPaymentAmount,
        decimal expectedPaymentAmount)
    {
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount} RequestedPaymentAmount={RequestedPaymentAmount} ExpectedPaymentAmount={ExpectedPaymentAmount}",
            "Checkout.PaymentValidated",
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            "PaymentAmountMismatch",
            cartId,
            userId,
            orderNumber,
            currencyCode,
            itemCount,
            paymentCount,
            totalAmount,
            requestedPaymentAmount,
            expectedPaymentAmount);
    }

    private static double ElapsedMilliseconds(long startedAt) =>
        Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 3);

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

}
