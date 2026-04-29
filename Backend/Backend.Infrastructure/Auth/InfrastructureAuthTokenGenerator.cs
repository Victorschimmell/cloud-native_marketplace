using Backend.Application.Common.Abstractions;
using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructureAuthTokenGenerator(
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider) : IAuthTokenGenerator
{
    private const string LocalTokenSecret = "local-development-token-secret-not-for-production-2026";

    public AuthTokenDto CreateToken(UserAccount userAccount)
    {
        var now = dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(GetTokenLifetimeMinutes());

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object>
        {
            ["sub"] = userAccount.Id.ToString(),
            ["email"] = userAccount.Email.Value,
            ["isAdmin"] = userAccount.IsAdmin,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = expiresAt.ToUnixTimeSeconds()
        };

        if (userAccount.IsAdmin)
        {
            payload["role"] = "Admin";
        }

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(unsignedToken);

        return new AuthTokenDto($"{unsignedToken}.{signature}", expiresAt);
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

        return LocalTokenSecret;
    }

    private int GetTokenLifetimeMinutes() =>
        int.TryParse(configuration["Authentication:AccessTokenLifetimeMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 60;

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
