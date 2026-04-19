using System.Net;
using System.Net.Http.Json;
using Backend.Api;
using Backend.Api.Contracts.Commerce.Payments;

namespace Backend.IntegrationTests;

public class PaymentsEndpointsTests : IClassFixture<MarketplaceApiFactory>
{
    private readonly HttpClient _client;

    public PaymentsEndpointsTests(MarketplaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPayment_ReturnsNotImplemented()
    {
        // Act
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/payments/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task RecordPayment_ReturnsNotImplemented()
    {
        // Arrange
        var recordRequest = new RecordPaymentRequest
        {
            OrderId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            PaymentInstallments = 1,
            PaymentValue = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", recordRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
