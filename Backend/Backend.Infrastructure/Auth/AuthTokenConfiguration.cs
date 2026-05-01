using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure.Auth;

public static class AuthTokenConfiguration
{
    public const string AuthenticationScheme = "MarketplaceBearer";

    private const string LocalTokenSecret = "local-development-token-secret-not-for-production-2026";

    public static SymmetricSecurityKey CreateSigningKey(IConfiguration configuration, string? environmentName = null) =>
        new(Encoding.UTF8.GetBytes(GetTokenSecret(configuration, environmentName)));

    public static TokenValidationParameters CreateTokenValidationParameters(
        IConfiguration configuration,
        string environmentName) =>
        new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSigningKey(configuration, environmentName),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };

    public static string GetTokenSecret(IConfiguration configuration, string? environmentName = null)
    {
        var secret = configuration["Authentication:TokenSecret"];

        if (!string.IsNullOrWhiteSpace(secret) && secret.Length >= 32)
        {
            return secret;
        }

        if (environmentName is null || IsLocalEnvironment(environmentName))
        {
            return LocalTokenSecret;
        }

        throw new InvalidOperationException("Authentication token secret must be configured and at least 32 characters long.");
    }

    public static int GetTokenLifetimeMinutes(IConfiguration configuration) =>
        int.TryParse(configuration["Authentication:AccessTokenLifetimeMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 60;

    private static bool IsLocalEnvironment(string environmentName) =>
        string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);
}
