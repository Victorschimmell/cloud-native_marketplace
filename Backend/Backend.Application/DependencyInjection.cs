using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Abstractions;
using Backend.Application.Interfaces.Services;
using Backend.Application.Services;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrencyConversionService, FixedRateCurrencyConversionService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutObservability, CheckoutObservability>();
        services.AddScoped<ICheckoutService, CheckoutService>();

        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminIssueService, AdminIssueService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISellerVerificationService, SellerVerificationService>();

        return services;
    }
}
