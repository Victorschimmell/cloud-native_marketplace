using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.Auth;
using Backend.Api.Contracts.User.Registration;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class AuthEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/registration/customer",
            new RegisterCustomerRequest
            {
                Email = email,
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                Phone = "+4512345678"
            },
            TestContext.Current.CancellationToken);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            IntegrationTestJson.Options,
            TestContext.Current.CancellationToken);
        Assert.NotNull(login);
        Assert.Equal(email, login.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(login.Token.AccessToken));
        Assert.True(login.Token.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email = "missing@example.com",
                Password = "Password123!"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
