using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Backend.Api.Auth;

internal sealed class MarketplaceBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration,
    IUserAccountRepository userAccountRepository,
    IDateTimeProvider dateTimeProvider,
    IHostEnvironment hostEnvironment)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "MarketplaceBearer";
    private const string LocalTokenSecret = "local-development-token-secret-not-for-production-2026";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorization["Bearer ".Length..].Trim();
        var principal = await ValidateTokenAsync(token);

        return principal is null
            ? AuthenticateResult.Fail("Invalid bearer token.")
            : AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }

    private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenParts = token.Split('.');

            if (tokenParts.Length != 3)
            {
                return null;
            }

            var unsignedToken = $"{tokenParts[0]}.{tokenParts[1]}";
            var expectedSignature = Sign(unsignedToken);

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expectedSignature),
                    Encoding.ASCII.GetBytes(tokenParts[2])))
            {
                return null;
            }

            using var payload = JsonDocument.Parse(Base64UrlDecode(tokenParts[1]));
            var root = payload.RootElement;

            if (!root.TryGetProperty("sub", out var subjectProperty) ||
                !Guid.TryParse(subjectProperty.GetString(), out var userId) ||
                !root.TryGetProperty("exp", out var expiresProperty))
            {
                return null;
            }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresProperty.GetInt64());
            if (expiresAt <= dateTimeProvider.UtcNow)
            {
                return null;
            }

            var userAccount = await userAccountRepository.GetByIdAsync(userId, Context.RequestAborted);
            if (userAccount is null ||
                userAccount.IsBlocked ||
                userAccount.AccountStatus is AccountStatus.Disabled or AccountStatus.Suspended ||
                userAccount.LockedUntilUtc is not null && userAccount.LockedUntilUtc > dateTimeProvider.UtcNow)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
                new(ClaimTypes.Email, userAccount.Email.Value)
            };

            if (userAccount.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string Sign(string value)
    {
        var secret = GetTokenSecret();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private string GetTokenSecret()
    {
        var secret = configuration["Authentication:TokenSecret"];

        if (!string.IsNullOrWhiteSpace(secret) && secret.Length >= 32)
        {
            return secret;
        }

        if (hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing"))
        {
            return LocalTokenSecret;
        }

        throw new InvalidOperationException("Authentication token secret must be configured and at least 32 characters long.");
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');

        return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
    }
}
