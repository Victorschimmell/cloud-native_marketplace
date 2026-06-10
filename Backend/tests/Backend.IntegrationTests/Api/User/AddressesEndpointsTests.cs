using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.User.Addresses;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

[Collection("PostgresDocker")]
public class AddressesEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;
    private readonly MarketplaceApiFactory _factory;

    public AddressesEndpointsTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAddress_WhenUserIsAnonymous_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync($"/api/addresses/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAddress_WhenAddressIsAuthenticatedCustomersDefault_ReturnsAddress()
    {
        // Arrange
        var (userId, addressId) = await SeedCustomerWithDefaultAddressAsync("get-default-address@example.com");
        AuthenticateAs(userId);

        // Act
        var response = await _client.GetAsync($"/api/addresses/{addressId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(address);
        Assert.Equal(addressId, address.Id);
        Assert.Equal("Amagerbrogade 42", address.AddressLine1);
        Assert.Equal("DK", address.CountryCode);
    }

    [Fact]
    public async Task GetAddress_WhenAddressBelongsToAnotherCustomer_ReturnsNotFound()
    {
        // Arrange
        var (ownerUserId, addressId) = await SeedCustomerWithDefaultAddressAsync("address-owner@example.com");
        var otherUserId = await SeedCustomerAsync("address-other@example.com");
        Assert.NotEqual(ownerUserId, otherUserId);
        AuthenticateAs(otherUserId);

        // Act
        var response = await _client.GetAsync($"/api/addresses/{addressId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAddress_WhenMakeDefaultIsTrue_PersistsAddressAndUpdatesCustomerDefault()
    {
        // Arrange
        var userId = await SeedCustomerAsync("create-address@example.com");
        AuthenticateAs(userId);
        var request = new CreateAddressRequest
        {
            AddressLine1 = "  Osterbrogade 7  ",
            AddressLine2 = "  2 tv  ",
            City = " Copenhagen ",
            State = " Capital Region ",
            PostalCode = " 2100 ",
            CountryCode = " dk ",
            MakeDefault = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/addresses", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(IntegrationTestJson.Options, TestContext.Current.CancellationToken);
        Assert.NotNull(address);
        Assert.Equal("Osterbrogade 7", address.AddressLine1);
        Assert.Equal("2 tv", address.AddressLine2);
        Assert.Equal("Copenhagen", address.City);
        Assert.Equal("Capital Region", address.State);
        Assert.Equal("2100", address.PostalCode);
        Assert.Equal("DK", address.CountryCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customer = await dbContext.Customers.SingleAsync(c => c.UserId == userId, TestContext.Current.CancellationToken);
        Assert.Equal(address.Id, customer.DefaultAddressId);
    }

    private async Task<(Guid UserId, Guid AddressId)> SeedCustomerWithDefaultAddressAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var address = TestEntityFactory.CreateAddress();
        address.AddressLine1 = "Amagerbrogade 42";
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id, address.Id);

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Addresses.Add(address);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (customerUser.Id, address.Id);
    }

    private async Task<Guid> SeedCustomerAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerUser = TestEntityFactory.CreateUserAccount(email);
        var customer = TestEntityFactory.CreateCustomer(customerUser.Id);

        dbContext.UserAccounts.Add(customerUser);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return customerUser.Id;
    }

    private void AuthenticateAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestAuth.CreateBearerToken(userId));
    }
}
