using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class CheckoutServiceObservabilityTests
{
    [Fact]
    public async Task CheckoutAsync_WhenKnownFailureOccurs_LogsTerminalCheckoutFailedEvent()
    {
        var observability = new FakeCheckoutObservability();
        var service = CreateService(observability);
        var request = new CheckoutRequest(
            CartId: null,
            UserId: null,
            SessionId: null,
            ShippingAddress: new CheckoutShippingAddressDto(
                PostalCode: "1000",
                City: "Copenhagen",
                State: "Capital Region",
                AddressLine1: "Main Street 1",
                AddressLine2: null,
                CountryCode: "DK"),
            SaveShippingAddressAsDefault: false,
            Payments: []);

        var result = await service.CheckoutAsync(request, "EUR", TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Contains(observability.Failures, failure =>
            failure.Operation == "Checkout.PaymentValidated" &&
            failure.ErrorType == "InvalidCurrency");
        Assert.Contains(observability.Failures, failure =>
            failure.Operation == "Checkout.Failed" &&
            failure.ErrorType == "InvalidCurrency");
    }

    private static CheckoutService CreateService(FakeCheckoutObservability observability) =>
        new(
            new FakeCartRepository(),
            new FakeProductListingRepository(),
            new FakeOrderRepository(),
            new FakeOrderItemRepository(),
            new FakeOrderNumberRepository(),
            new FakeCustomerRepository(),
            new FakeSellerRepository(),
            new FakeAddressRepository(),
            new FakePaymentService(),
            new FakeDateTimeProvider(),
            new FakeCurrencyConversionService(),
            new FakeAuditLogService(),
            new FakeUnitOfWork(),
            observability);
}
