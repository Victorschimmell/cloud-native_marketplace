namespace Backend.Application.Interfaces.Services;

public interface ICheckoutObservability
{
    long GetTimestamp();
    void Started(long startedAt, CheckoutObservabilityContext context);
    void Succeeded(string operation, long startedAt, CheckoutObservabilityContext context);
    void Failed(string operation, string errorType, long startedAt, CheckoutObservabilityContext context);
    void InventoryFailed(long startedAt, CheckoutObservabilityContext context, Guid listingId, int requestedQuantity, int availableQuantity);
    void PaymentAmountFailed(long startedAt, CheckoutObservabilityContext context, decimal requestedPaymentAmount, decimal expectedPaymentAmount);
    void Unexpected(Exception exception, long startedAt, CheckoutObservabilityContext context);
}

public sealed record CheckoutObservabilityContext(
    Guid? UserId = null,
    Guid? CartId = null,
    Guid? OrderId = null,
    string? OrderNumber = null,
    string? CurrencyCode = null,
    int? ItemCount = null,
    int? PaymentCount = null,
    decimal? TotalAmount = null);
