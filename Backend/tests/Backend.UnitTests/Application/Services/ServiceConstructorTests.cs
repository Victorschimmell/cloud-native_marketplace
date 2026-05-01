using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceConstructorTests
{
    [Fact]
    public void ProductService_Throws_When_ProductRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductService(null!, new FakeProductListingRepository(), new FakeCurrencyConversionService()));
    }

    [Fact]
    public void CartService_Throws_When_ProductListingRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new CartService(new FakeCartRepository(), null!, new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeUnitOfWork()));
    }

    [Fact]
    public void AuthService_Throws_When_PasswordHasher_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthService(new FakeUserAccountRepository(), null!, new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeUnitOfWork()));
    }

    [Fact]
    public void Application_Services_Can_Be_Constructed_With_Application_Only_Fakes()
    {
        _ = new CustomerService(new FakeCustomerRepository());
        _ = new SellerService(new FakeSellerRepository());
        _ = new ProductService(new FakeProductRepository(), new FakeProductListingRepository(), new FakeCurrencyConversionService());
        _ = new CategoryService(new FakeProductCategoryRepository());
        _ = new OrderService(new FakeOrderRepository(), new FakeOrderItemRepository());
        var paymentService = new PaymentService(new FakePaymentRepository(), new FakeOrderRepository(), new FakeCurrencyRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new ReviewService(new FakeOrderReviewRepository(), new FakeOrderRepository(), new FakeDateTimeProvider());
        _ = new CartService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeUnitOfWork());
        _ = new CheckoutService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeOrderRepository(), new FakeOrderNumberRepository(), new FakeCustomerRepository(), paymentService, new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeUnitOfWork());
        _ = new AnalyticsService(new FakeOrderRepository(), new FakeDateTimeProvider());
        _ = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new RegistrationService(
            new FakeUserAccountRepository(),
            new FakeCustomerRepository(),
            new FakeSellerRepository(),
            new FakeSellerVerificationRequestRepository(),
            new FakePasswordHasher(),
            new FakeAuthTokenGenerator(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());
        _ = new AdminService(new FakeUserAccountRepository(), new FakeAuditLogRepository());
        _ = new SellerVerificationService(new FakeSellerVerificationRequestRepository(), new FakeSellerRepository(), new FakeUserAccountRepository(), new FakeCurrentUserProvider(), new FakeDateTimeProvider());
        _ = new ShipmentService(new FakeShipmentRepository(), new FakeOrderRepository(), new FakeDateTimeProvider());
        _ = new AuditLogService(new FakeAuditLogRepository(), new FakeDateTimeProvider());
    }
}

