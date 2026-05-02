using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
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
using System.Text.Json.Serialization;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class CheckoutEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly JsonSerializerOptions _jsonOptions;

    public CheckoutEndpointsTests(MarketplaceApiFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _testOutputHelper = testOutputHelper;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
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

        var previewLines = await response.Content.ReadFromJsonAsync<CheckoutPreviewLineResponse[]>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(previewLines);

        var line = Assert.Single(previewLines);
        Assert.Equal(listingId, line.ListingId);
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
            ShippingAddressId = Guid.NewGuid(),
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
        var addressId = await SeedAddressAsync();
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = Guid.NewGuid(),
            ShippingAddressId = addressId,
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
            ShippingAddressId = Guid.NewGuid(),
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
    public async Task Checkout_WhenCartExists_ReturnsApprovedOrderAndRecordsPayment()
    {
        // Arrange
        var addressId = await SeedAddressAsync();
        var (_, userId, cartId) = await SeedCartWithItemAsync("checkout-fail@example.com", "Checkout product", "CHECKOUT-002", 100m, 1, addressId);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddressId = addressId,
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

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);

        Assert.Equal(200.00m, checkout.TotalAmount);

        var order = checkout.Order;
        // TODO: Currently the order using customerId as return value, however, the other endpoints are using userId
        //          We should decide on a consistent approach for this, either add a new field userId in Order and return userId rather than customerId
        //          or modify the other endpoints to use customerId instead of userId
        // Assert.Equal(userId, order.CustomerId);
        Assert.Equal(addressId, order.ShippingAddressId);
        Assert.Equal(cartId, order.PlacedFromCartId);
        Assert.Equal(OrderStatus.Approved, order.OrderStatus);
        Assert.Equal(100.00m, order.SubtotalAmount);
        Assert.Equal(100.00m, order.FreightAmount);
        Assert.Equal(200.00m, order.TotalAmount);
        Assert.Equal("BRL", order.CurrencyCode);

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
    }

    [Fact]
    public async Task Checkout_WhenShippingAddressIsNotCustomersDefault_ReturnsBadRequest()
    {
        // Arrange
        var defaultAddressId = await SeedAddressAsync();
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-address@example.com", "Checkout address product", "CHECKOUT-ADDRESS-001", 100m, 1, defaultAddressId);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddressId = Guid.NewGuid(),
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

        using var responseJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            "Shipping address is not available for the authenticated customer.",
            responseJson.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Checkout_WhenCurrencyIsUsd_ValidatesConvertedPaymentTotal()
    {
        // Arrange
        var addressId = await SeedAddressAsync();
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-usd@example.com", "Checkout USD product", "CHECKOUT-USD-001", 100m, 1, addressId);
        var currencyId = await SeedCurrencyAsync("USD", "US Dollar");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddressId = addressId,
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

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);
        Assert.Equal(36.00m, checkout.TotalAmount);
        Assert.Equal("USD", checkout.Order.CurrencyCode);
        Assert.Equal(18.00m, checkout.Order.SubtotalAmount);
        Assert.Equal(18.00m, checkout.Order.FreightAmount);
        Assert.Equal(36.00m, checkout.Order.TotalAmount);
        Assert.Equal(36.00m, Assert.Single(checkout.Payments).PaymentValue);
    }

    [Fact]
    public async Task Checkout_WhenCartIdIsOmitted_UsesAuthenticatedUsersActiveCart()
    {
        // Arrange
        var addressId = await SeedAddressAsync();
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-current@example.com", "Checkout current product", "CHECKOUT-CURRENT-001", 100m, 1, addressId);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            ShippingAddressId = addressId,
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
        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(checkout);
        Assert.Equal(cartId, checkout.Cart.Id);
        Assert.Equal(CartStatus.Converted, checkout.Cart.Status);
    }

    [Fact]
    public async Task Checkout_WhenPaymentAmountIsInsufficient_ReturnsBadRequest()
    {
        // Arrange
        var addressId = await SeedAddressAsync();
        var (_, _, cartId) = await SeedCartWithItemAsync("checkout-success@example.com", "Checkout product", "CHECKOUT-001", 100m, 1, addressId);
        var currencyId = await SeedCurrencyAsync("BRL", "Brazilian Real");
        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartId,
            ShippingAddressId = addressId,
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

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(_jsonOptions, TestContext.Current.CancellationToken);
        Assert.NotNull(cart);

        return (listingId, userId, cart.Id);
    }

    private async Task<Guid> SeedAddressAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var address = TestEntityFactory.CreateAddress();

        dbContext.Addresses.Add(address);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return address.Id;
    }

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
