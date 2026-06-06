using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Enums;
using Backend.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Backend.Api.Auth;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddMarketplaceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            AuthTokenConfiguration.GetTokenSecret(configuration, environment.EnvironmentName);
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();
        services
            .AddAuthentication(AuthTokenConfiguration.AuthenticationScheme)
            .AddJwtBearer(AuthTokenConfiguration.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = AuthTokenConfiguration.CreateTokenValidationParameters(
                    configuration,
                    environment.EnvironmentName);
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateUserAccountAsync
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"));
        });

        return services;
    }

    private static async Task ValidateUserAccountAsync(TokenValidatedContext context)
    {
        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Fail("Invalid bearer token.");
            return;
        }

        var userAccountRepository = context.HttpContext.RequestServices.GetRequiredService<IUserAccountRepository>();
        var dateTimeProvider = context.HttpContext.RequestServices.GetRequiredService<IDateTimeProvider>();
        var userAccount = await userAccountRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        if (userAccount is null ||
            userAccount.IsBlocked ||
            userAccount.AccountStatus is AccountStatus.Disabled or AccountStatus.Suspended ||
            userAccount.LockedUntilUtc is not null && userAccount.LockedUntilUtc > dateTimeProvider.UtcNow)
        {
            context.Fail("Invalid bearer token.");
        }
    }
}
