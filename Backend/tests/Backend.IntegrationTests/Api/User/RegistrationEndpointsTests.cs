using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.Registration;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class RegistrationEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public RegistrationEndpointsTests(MarketplaceApiFactory factory)
    {
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
