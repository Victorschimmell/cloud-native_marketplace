using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.Registration;

namespace Backend.IntegrationTests;

public class RegistrationEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public RegistrationEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsNotImplemented()
    {
        // Arrange
        var registerRequest = new RegisterCustomerRequest
        {
            Email = "customer@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+4512345678"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/customer", registerRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task RegisterSeller_ReturnsNotImplemented()
    {
        // Arrange
        var registerRequest = new RegisterSellerRequest
        {
            Email = "seller@example.com",
            Password = "Password123!",
            BusinessName = "My Business",
            RegistrationNumber = "+4512345678",
            PayoutInformation = "Bank Account Info"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/registration/seller", registerRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
