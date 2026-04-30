using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Enums;
using Backend.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
    public const string SchemeName = AuthTokenConfiguration.AuthenticationScheme;
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

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
            var validationParameters = AuthTokenConfiguration.CreateTokenValidationParameters(
                configuration,
                hostEnvironment.EnvironmentName);
            validationParameters.LifetimeValidator = ValidateLifetime;

            var principal = TokenHandler.ValidateToken(token, validationParameters, out _);
            var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(subject, out var userId))
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
        catch (ArgumentException)
        {
            return null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private bool ValidateLifetime(
        DateTime? notBefore,
        DateTime? expires,
        SecurityToken securityToken,
        TokenValidationParameters validationParameters)
    {
        var now = dateTimeProvider.UtcNow.UtcDateTime;

        return (notBefore is null || notBefore <= now) &&
            expires is not null &&
            expires > now;
    }
}
