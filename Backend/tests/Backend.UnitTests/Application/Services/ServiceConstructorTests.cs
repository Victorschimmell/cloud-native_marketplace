using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceConstructorTests
{
    [Fact]
    public void ProductService_Throws_When_ProductRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductService(null!, new FakeUnitOfWork()));
    }

    [Fact]
    public void CartService_Throws_When_ProductListingRepository_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new CartService(new FakeCartRepository(), null!, new FakeDateTimeProvider(), new FakeUnitOfWork()));
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
        _ = new ProductService(new FakeProductRepository(), new FakeUnitOfWork());
        _ = new CategoryService(new FakeProductCategoryRepository(), new FakeUnitOfWork());
        _ = new OrderService(new FakeOrderRepository(), new FakeOrderItemRepository(), new FakeUnitOfWork());
        _ = new PaymentService(new FakePaymentRepository(), new FakeOrderRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new ReviewService(new FakeReviewRepository(), new FakeOrderRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new CartService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new CheckoutService(new FakeCartRepository(), new FakeOrderRepository(), new FakePaymentRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new AnalyticsService(new FakeOrderRepository(), new FakeDateTimeProvider());
        _ = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new RegistrationService(new FakeUserAccountRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakePasswordHasher(), new FakeUnitOfWork());
        _ = new AdminService(new FakeUserAccountRepository(), new FakeAuditLogRepository(), new FakeUnitOfWork());
        _ = new SellerVerificationService(new FakeSellerVerificationRequestRepository(), new FakeSellerRepository(), new FakeCurrentUserProvider(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new ShipmentService(new FakeShipmentRepository(), new FakeOrderRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());
        _ = new AuditLogService(new FakeAuditLogRepository(), new FakeDateTimeProvider());
    }
}
