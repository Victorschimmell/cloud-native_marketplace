using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api.Contracts.Commerce.Cart;
using Backend.Api.Contracts.Commerce.Checkout;
using Backend.Api.Contracts.Commerce.Payments;
using Backend.Api.Contracts.Commerce.Orders;
using DomainEnums = Backend.Domain.Enums;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CheckoutEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly JsonSerializerOptions _jsonOptions = IntegrationTestJson.Options;

    public CheckoutEndpointsTests(MarketplaceApiFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task PreviewCheckout_WhenUserIsAnonymous_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/checkout/preview?currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PreviewCheckout_WhenCartDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = await SeedCustomerAsync("missing-preview@example.com");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/checkout/preview?cartId={Guid.NewGuid()}&currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PreviewCheckout_WhenCurrencyIsUnsupported_ReturnsBadRequest()
    {
        // Arrange
        var (_, userId, cartId) = await SeedCartWithItemAsync("preview-currency@example.com", "Preview currency product", "PREVIEW-CURRENCY-001", 100m, 2);

        // Act
        var response = await _client.GetAsync($"/api/checkout/preview?cartId={cartId}&currency=EUR", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PreviewCheckout_WhenCartExists_ReturnsConvertedPreviewLines()
    {
        // Arrange
        var (listingId, _, cartId) = await SeedCartWithItemAsync("preview-success@example.com", "Preview product", "PREVIEW-001", 100m, 2);

        // Act
        var response = await _client.GetAsync($"/api/checkout/preview?cartId={cartId}&currency=USD", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var preview = await response.Content.ReadFromJsonAsync<CheckoutPreviewResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(preview);
        Assert.Equal(36.00m, preview.SubtotalAmount);
        Assert.Equal(18.00m, preview.FreightAmount);
        Assert.Equal(54.00m, preview.TotalAmount);
        Assert.Equal("USD", preview.CurrencyCode);

        var line = Assert.Single(preview.Lines);
        Assert.Equal(listingId, line.ListingId);
        Assert.NotEqual(Guid.Empty, line.ProductId);
        Assert.Equal("Preview product", line.ProductName);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(18.00m, line.UnitPrice);
        Assert.Equal(36.00m, line.LineTotal);
        Assert.Equal("USD", line.CurrencyCode);
    }

    [Fact]
    public async Task Checkout_WhenUserIsAnonymous_ReturnsUnauthorized()
    {
        // Arrange
        var checkoutRequest = new CheckoutRequest
        {
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = Array.Empty<RecordPaymentRequest>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenCartDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = await SeedCustomerAsync("missing-checkout@example.com");
        AuthenticateAs(userId);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = Guid.NewGuid(),
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenCurrencyIsUnsupported_ReturnsBadRequest()
    {
        // Arrange
        var userId = await SeedCustomerAsync("unsupported-currency@example.com");
        AuthenticateAs(userId);
        var checkoutRequest = new CheckoutRequest
        {
            CartId = Guid.NewGuid(),
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = Guid.NewGuid(),
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=EUR", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenCartExists_ReturnsPendingOrderAndRecordsPayment()
    {
        // Arrange
        var (listingId, userId, cartId) = await SeedCartWithItemAsync("checkout-fail@example.com", "Checkout product", "CHECKOUT-002", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var shippingAddress = CreateShippingAddressRequest(
            addressLine1: "Norrebrogade 12",
            postalCode: "2200",
            city: "Copenhagen",
            state: "Capital Region",
            countryCode: "dk");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = shippingAddress,
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);

        Assert.Equal(200.00m, checkout.TotalAmount);

        var order = checkout.Order;
        Assert.Equal(userId, order.UserId);
        Assert.NotEqual(Guid.Empty, order.ShippingAddressId);
        Assert.Equal(cartId, order.PlacedFromCartId);
        Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        Assert.Equal(100.00m, order.SubtotalAmount);
        Assert.Equal(100.00m, order.FreightAmount);
        Assert.Equal(200.00m, order.TotalAmount);
        Assert.Equal("BRL", order.CurrencyCode);
        var orderItem = Assert.Single(order.Items);
        Assert.Equal(order.Id, orderItem.OrderId);
        Assert.Equal(1, orderItem.OrderItemId);
        Assert.Equal(listingId, orderItem.ListingId);
        Assert.Equal("Checkout product", orderItem.ProductName);
        Assert.Equal("MarketplaceTraders", orderItem.SellerName);
        Assert.Equal(1, orderItem.Quantity);
        Assert.Equal(100.00m, orderItem.UnitPrice);
        Assert.Equal(100.00m, orderItem.LineTotal);
        Assert.Equal(100.00m, orderItem.FreightValue);
        Assert.Equal("BRL", orderItem.CurrencyCode);

        var cart = checkout.Cart;
        Assert.Equal(cartId, cart.Id);
        Assert.Equal(CartStatus.Converted, cart.Status);
        Assert.Single(cart.Items);
        var cartItem = Assert.Single(cart.Items);
        Assert.Equal(cartId, cartItem.CartId);
        Assert.Equal(1, cartItem.Quantity);
        Assert.Equal(100.00m, cartItem.UnitPriceAtAddition);
        Assert.Equal("BRL", cartItem.CurrencyCode);

        var payment = Assert.Single(checkout.Payments);
        Assert.Equal(order.Id, payment.OrderId);
        Assert.Equal(currencyId, payment.CurrencyId);
        Assert.Equal(PaymentType.CreditCard, payment.PaymentType);
        Assert.Equal(1, payment.PaymentInstallments);
        Assert.Equal(200.00m, payment.PaymentValue);
        Assert.Equal(PaymentStatus.Paid, payment.PaymentStatus);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customer = await dbContext.Customers.SingleAsync(c => c.UserId == userId, TestContext.Current.CancellationToken);
        var persistedAddress = await dbContext.Addresses.SingleAsync(a => a.Id == order.ShippingAddressId, TestContext.Current.CancellationToken);
        var persistedOrderItem = await dbContext.OrderItems.SingleAsync(i => i.OrderId == order.Id, TestContext.Current.CancellationToken);
        var persistedListing = await dbContext.ProductListings.SingleAsync(l => l.Id == listingId, TestContext.Current.CancellationToken);
        Assert.Null(customer.DefaultAddressId);
        Assert.Equal("Norrebrogade 12", persistedAddress.AddressLine1);
        Assert.Equal("2200", persistedAddress.PostalCode);
        Assert.Equal("Copenhagen", persistedAddress.City);
        Assert.Equal("Capital Region", persistedAddress.State);
        Assert.Equal("DK", persistedAddress.CountryCode);
        Assert.Equal(listingId, persistedOrderItem.ListingId);
        Assert.Equal(1, persistedOrderItem.Quantity);
        Assert.Equal(100.00m, persistedOrderItem.UnitPrice);
        Assert.Equal(100.00m, persistedOrderItem.FreightValue);
        Assert.Equal(9, persistedListing.InventoryQuantity);
    }

    [Fact]
    public async Task Checkout_WhenSaveShippingAddressAsDefaultIsTrue_UpdatesCustomerDefaultAddress()
    {
        // Arrange
        var (_, userId, cartId) = await SeedCartWithItemAsync("checkout-save-default@example.com", "Checkout save default product", "CHECKOUT-SAVE-DEFAULT-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(),
            SaveShippingAddressAsDefault = true,
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customer = await dbContext.Customers.SingleAsync(c => c.UserId == userId, TestContext.Current.CancellationToken);
        Assert.Equal(checkout.Order.ShippingAddressId, customer.DefaultAddressId);
    }

    [Fact]
    public async Task Checkout_WhenShippingAddressLineOneIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-address@example.com", "Checkout address product", "CHECKOUT-ADDRESS-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(addressLine1: " "),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenCurrencyIsUsd_ValidatesConvertedPaymentTotal()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-usd@example.com", "Checkout USD product", "CHECKOUT-USD-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("USD", "US Dollar");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 36m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=USD", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);
        Assert.Equal(36.00m, checkout.TotalAmount);
        Assert.Equal("USD", checkout.Order.CurrencyCode);
        Assert.Equal(18.00m, checkout.Order.SubtotalAmount);
        Assert.Equal(18.00m, checkout.Order.FreightAmount);
        Assert.Equal(36.00m, checkout.Order.TotalAmount);
        Assert.Equal(36.00m, Assert.Single(checkout.Payments).PaymentValue);
    }

    [Fact]
    public async Task Checkout_WhenCurrencyIsDkk_ReturnsOrderItemLineTotalFromBasePriceTotal()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-dkk-rounding@example.com", "Checkout DKK product", "CHECKOUT-DKK-ROUNDING-001", 118.58m, 10);
        var currencyId = await SeedCurrencyAsync("DKK", "Danish Krone");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 1504.39m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=DKK", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);
        Assert.Equal("DKK", checkout.Order.CurrencyCode);
        Assert.Equal(1387.39m, checkout.Order.SubtotalAmount);
        Assert.Equal(117.00m, checkout.Order.FreightAmount);
        Assert.Equal(1504.39m, checkout.Order.TotalAmount);

        var orderItem = Assert.Single(checkout.Order.Items);
        Assert.Equal(10, orderItem.Quantity);
        Assert.Equal(138.74m, orderItem.UnitPrice);
        Assert.Equal(1387.39m, orderItem.LineTotal);
        Assert.Equal("DKK", orderItem.CurrencyCode);
    }

    [Fact]
    public async Task Checkout_WhenCartIdIsOmitted_UsesAuthenticatedUsersActiveCart()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-current@example.com", "Checkout current product", "CHECKOUT-CURRENT-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);
        Assert.Equal(cartId, checkout.Cart.Id);
        Assert.Equal(CartStatus.Converted, checkout.Cart.Status);
    }

    [Fact]
    public async Task Checkout_WhenPaymentAmountIsInsufficient_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-success@example.com", "Checkout product", "CHECKOUT-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.CreditCard,
                    PaymentInstallments = 1,
                    PaymentValue = 190m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenPaymentTypeIsNotCreditCard_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-payment-type@example.com", "Checkout payment type product", "CHECKOUT-PAYMENT-TYPE-001", 100m, 1);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddress = CreateShippingAddressRequest(),
            Payments = new[]
            {
                new RecordPaymentRequest
                {
                    CurrencyId = currencyId,
                    PaymentType = PaymentType.DebitCard,
                    PaymentInstallments = 1,
                    PaymentValue = 200m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/checkout?currency=BRL", checkoutRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var responseJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            "Only credit card payments are supported at checkout.",
            responseJson.RootElement.GetProperty("detail").GetString());
    }

    private async Task<Guid> SeedProductListingAsync(
        string productName,
        string sku,
        decimal price,
        int inventoryQuantity = 10,
        bool isDeleted = false,
        DomainEnums.ListingVisibilityStatus visibilityStatus = DomainEnums.ListingVisibilityStatus.Published)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sellerUser = TestEntityFactory.CreateUserAccount($"{Guid.NewGuid():N}@seller.example");
        var seller = TestEntityFactory.CreateSeller(sellerUser.Id);
        var category = TestEntityFactory.CreateCategory("checkout_category", "Checkout category");
        var product = TestEntityFactory.CreateProduct(category.Id, productName);
        var listing = TestEntityFactory.CreateListing(seller.Id, product.Id, sku, price);
        listing.InventoryQuantity = inventoryQuantity;
        listing.IsDeleted = isDeleted;
        listing.VisibilityStatus = visibilityStatus;

        dbContext.UserAccounts.Add(sellerUser);
        dbContext.Sellers.Add(seller);
        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductListings.Add(listing);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return listing.Id;
    }

    private async Task<Guid> SeedCustomerAsync(string email, Guid? defaultAddressId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id, defaultAddressId);

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return customerUser.Id;
    }

    private async Task<(Guid ListingId, Guid UserId, Guid CartId)> SeedCartWithItemAsync(
        string email,
        string productName,
        string sku,
        decimal price,
        int quantity,
        Guid? defaultAddressId = null)
    {
        var listingId = await SeedProductListingAsync(productName, sku, price);
        var userId = await SeedCustomerAsync(email, defaultAddressId);
        AuthenticateAs(userId);

        var addItemRequest = new AddCartItemRequest
        {
            ListingId = listingId,
            Quantity = quantity
        };

        var response = await _client.PostAsJsonAsync("/api/cart/items?displayCurrency=BRL", addItemRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(cart);

        return (listingId, userId, cart.Id);
    }

    private static CheckoutShippingAddressRequest CreateShippingAddressRequest(
        string addressLine1 = "Amagerbrogade 42",
        string? addressLine2 = null,
        string city = "Copenhagen",
        string state = "Capital Region",
        string postalCode = "2300",
        string countryCode = "DK") =>
        new()
        {
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            State = state,
            PostalCode = postalCode,
            CountryCode = countryCode
        };

    private async Task<Guid> SeedCurrencyAsync(string code, string name)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var normalizedCode = code.Trim().ToUpperInvariant();
        var existingCurrency = await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Code == normalizedCode, TestContext.Current.CancellationToken);

        if (existingCurrency is not null)
        {
            return existingCurrency.Id;
        }

        var currency = TestEntityFactory.CreateCurrency(code, name);
        currency.Code = normalizedCode;

        dbContext.Currencies.Add(currency);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return currency.Id;
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }
}
