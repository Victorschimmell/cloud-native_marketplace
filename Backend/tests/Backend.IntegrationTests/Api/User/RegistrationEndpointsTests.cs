using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.Registration;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class RegistrationEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public RegistrationEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsRegisteredCustomer()
    {
        // Arrange
        var registerRequest = new RegisterCustomerRequest
        {
            Email = $"{Guid.NewGuid():N}@customer.example",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+4512345678"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/customer", registerRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(registration);
        Assert.NotNull(registration.Customer);
        Assert.Null(registration.Seller);
        Assert.Equal(registerRequest.Email, registration.User.Email);
        Assert.Equal(registerRequest.FirstName, registration.Customer.FirstName);
        Assert.NotNull(registration.Token);
        Assert.False(string.IsNullOrWhiteSpace(registration.Token.AccessToken));
    }

    [Fact]
    public async Task RegisterSeller_ReturnsRegisteredSeller()
    {
        // Arrange
        var registerRequest = new RegisterSellerRequest
        {
            Email = $"{Guid.NewGuid():N}@seller.example",
            Password = "Password123!",
            BusinessName = "My Business",
            RegistrationNumber = "+4512345678",
            PayoutInformation = "Bank Account Info"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/seller", registerRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(registration);
        Assert.Null(registration.Customer);
        Assert.NotNull(registration.Seller);
        Assert.Equal(registerRequest.Email, registration.User.Email);
        Assert.Equal(registerRequest.BusinessName, registration.Seller.BusinessName);
        Assert.Null(registration.Token);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var verificationRequestExists = await dbContext.SellerVerificationRequests
            .AnyAsync(request => request.SellerId == registration.Seller.Id, TestContext.Current.CancellationToken);
        Assert.True(verificationRequestExists);
    }

    [Fact]
    public async Task RegisterCustomer_WithExistingEmail_ReturnsConflict()
    {
        var registerRequest = new RegisterCustomerRequest
        {
            Email = $"{Guid.NewGuid():N}@customer.example",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+4512345678"
        };

        await _client.PostAsJsonAsync("/api/registration/customer", registerRequest, TestContext.Current.CancellationToken);

        var response = await _client.PostAsJsonAsync("/api/registration/customer", registerRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
