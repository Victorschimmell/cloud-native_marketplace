using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Abstractions;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Application.Services;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IShipmentService, ShipmentService>();

        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISellerService, SellerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISellerVerificationService, SellerVerificationService>();

        // IMPORTANT: Replace the following fake implementations with actual implementations when they are ready.
        services.AddScoped<IPaymentRepository, FakePaymentRepository>();
        services.AddScoped<IDateTimeProvider, FakeDateTimeProvider>();
        services.AddScoped<IPasswordHasher, FakePasswordHasher>();
        services.AddScoped<ICurrentUserProvider, FakeCurrentUserProvider>();
        services.AddScoped<IAuthTokenGenerator, FakeAuthTokenGenerator>();

        return services;
    }
}
