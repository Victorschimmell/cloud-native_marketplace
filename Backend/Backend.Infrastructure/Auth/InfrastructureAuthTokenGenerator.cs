using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Application.Common.Abstractions;
using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructureAuthTokenGenerator(
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider) : IAuthTokenGenerator
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    public AuthTokenDto CreateToken(UserAccount userAccount)
    {
        var now = dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(AuthTokenConfiguration.GetTokenLifetimeMinutes(configuration));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userAccount.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, userAccount.Email.Value),
            new("isAdmin", userAccount.IsAdmin.ToString())
        };

        if (userAccount.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                AuthTokenConfiguration.CreateSigningKey(configuration),
                SecurityAlgorithms.HmacSha256)
        };

        return new AuthTokenDto(TokenHandler.WriteToken(TokenHandler.CreateToken(descriptor)), expiresAt);
    }
}
