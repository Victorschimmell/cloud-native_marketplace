using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceConstructorTests
{
    [Fact]
    public void ProductService_Throws_When_ProductRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductService(null!, new FakeProductListingRepository(), new FakeProductCategoryRepository(), new FakeSellerRepository(), new FakeCurrentUserProvider(), new FakeAuditLogService(), new FakeUnitOfWork(), new FakeCurrencyConversionService()));
    }

    [Fact]
    public void CartService_Throws_When_ProductListingRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new CartService(new FakeCartRepository(), null!, new FakeCustomerRepository(), new FakeSellerRepository(), new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeUnitOfWork()));
    }

    [Fact]
    public void AuthService_Throws_When_PasswordHasher_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthService(new FakeUserAccountRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), null!, new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork()));
    }

    [Fact]
    public void Application_Services_Can_Be_Constructed_With_Application_Only_Fakes()
    {
        _ = new CustomerService(new FakeCustomerRepository());
        _ = new ProductService(new FakeProductRepository(), new FakeProductListingRepository(), new FakeProductCategoryRepository(), new FakeSellerRepository(), new FakeCurrentUserProvider(), new FakeAuditLogService(), new FakeUnitOfWork(), new FakeCurrencyConversionService());
        _ = new CategoryService(new FakeProductCategoryRepository());
        _ = new OrderService(new FakeOrderRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakeCurrencyConversionService(), new FakeAuditLogService(), new FakeUnitOfWork(), new FakeDateTimeProvider());
        _ = new PaymentService(new FakePaymentRepository(), new FakeOrderRepository(), new FakeCurrencyRepository(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork());
        _ = new ReviewService(new FakeOrderReviewRepository(), new FakeOrderRepository(), new FakeCustomerRepository(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork());
        _ = new CartService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeUnitOfWork());
        _ = new CheckoutService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeOrderRepository(), new FakeOrderItemRepository(), new FakeOrderNumberRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakeAddressRepository(), new FakePaymentService(), new FakeDateTimeProvider(), new FakeCurrencyConversionService(), new FakeAuditLogService(), new FakeUnitOfWork(), new FakeCheckoutObservability());
        _ = new AddressService(new FakeAddressRepository(), new FakeCustomerRepository(), new FakeUnitOfWork());
        _ = new AnalyticsService(new FakeOrderRepository(), new FakeDateTimeProvider(), new FakeCurrentUserProvider());
        _ = new AuthService(new FakeUserAccountRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork());
        _ = new RegistrationService(
            new FakeUserAccountRepository(),
            new FakeCustomerRepository(),
            new FakeSellerRepository(),
            new FakeSellerVerificationRequestRepository(),
            new FakePasswordHasher(),
            new FakeAuthTokenGenerator(),
            new FakeDateTimeProvider(),
            new FakeAuditLogService(),
            new FakeUnitOfWork());
        _ = new AdminService(
            new FakeUserAccountRepository(),
            new FakeAuditLogRepository(),
            new FakeAuditLogService(),
            new FakePaymentRepository(),
            new FakeCurrencyConversionService(),
            new FakeCurrentUserProvider(),
            new FakeUnitOfWork());
        _ = new AdminDashboardService(new FakeAdminDashboardRepository(), new FakeCurrencyConversionService(), new FakeDateTimeProvider(), new FakeCurrentUserProvider());
        _ = new SellerVerificationService(new FakeSellerVerificationRequestRepository(), new FakeSellerRepository(), new FakeCurrentUserProvider(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork());
        _ = new AuditLogService(new FakeAuditLogRepository(), new FakeDateTimeProvider(), new FakeCurrentUserProvider());
    }
}

