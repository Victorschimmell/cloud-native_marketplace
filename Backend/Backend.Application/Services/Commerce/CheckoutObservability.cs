using System.Diagnostics;
using Backend.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Services;

public sealed class CheckoutObservability : ICheckoutObservability
{
    private const string ComponentName = "CheckoutService";
    private readonly ILogger<CheckoutObservability> _logger;

    public CheckoutObservability(ILogger<CheckoutObservability> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public long GetTimestamp() => Stopwatch.GetTimestamp();

    public void Started(long startedAt, CheckoutObservabilityContext context) =>
        LogInformation("Checkout.Process", "Started", startedAt, context);

    public void Succeeded(string operation, long startedAt, CheckoutObservabilityContext context) =>
        LogInformation(operation, "Succeeded", startedAt, context);

    public void Failed(string operation, string errorType, long startedAt, CheckoutObservabilityContext context) =>
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderId={OrderId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount}",
            operation,
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            errorType,
            context.CartId,
            context.UserId,
            context.OrderId,
            context.OrderNumber,
            context.CurrencyCode,
            context.ItemCount,
            context.PaymentCount,
            context.TotalAmount);

    public void InventoryFailed(long startedAt, CheckoutObservabilityContext context, Guid listingId, int requestedQuantity, int availableQuantity) =>
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} ListingId={ListingId} RequestedQuantity={RequestedQuantity} AvailableQuantity={AvailableQuantity}",
            "Checkout.InventoryValidated",
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            "InsufficientInventory",
            context.CartId,
            context.UserId,
            context.OrderNumber,
            context.CurrencyCode,
            context.ItemCount,
            context.PaymentCount,
            listingId,
            requestedQuantity,
            availableQuantity);

    public void PaymentAmountFailed(long startedAt, CheckoutObservabilityContext context, decimal requestedPaymentAmount, decimal expectedPaymentAmount) =>
        _logger.LogWarning(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount} RequestedPaymentAmount={RequestedPaymentAmount} ExpectedPaymentAmount={ExpectedPaymentAmount}",
            "Checkout.PaymentValidated",
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            "PaymentAmountMismatch",
            context.CartId,
            context.UserId,
            context.OrderNumber,
            context.CurrencyCode,
            context.ItemCount,
            context.PaymentCount,
            context.TotalAmount,
            requestedPaymentAmount,
            expectedPaymentAmount);

    public void Unexpected(Exception exception, long startedAt, CheckoutObservabilityContext context) =>
        _logger.LogError(
            exception,
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. ErrorType={ErrorType} CartId={CartId} UserId={UserId} CurrencyCode={CurrencyCode}",
            "Checkout.Process",
            "Failed",
            ComponentName,
            ElapsedMilliseconds(startedAt),
            "UnexpectedException",
            context.CartId,
            context.UserId,
            context.CurrencyCode);

    private void LogInformation(string operation, string outcome, long startedAt, CheckoutObservabilityContext context) =>
        _logger.LogInformation(
            "Checkout observability event {Operation} {Outcome} for {Component} in {DurationMs} ms. CartId={CartId} UserId={UserId} OrderId={OrderId} OrderNumber={OrderNumber} CurrencyCode={CurrencyCode} ItemCount={ItemCount} PaymentCount={PaymentCount} TotalAmount={TotalAmount}",
            operation,
            outcome,
            ComponentName,
            ElapsedMilliseconds(startedAt),
            context.CartId,
            context.UserId,
            context.OrderId,
            context.OrderNumber,
            context.CurrencyCode,
            context.ItemCount,
            context.PaymentCount,
            context.TotalAmount);

    private static double ElapsedMilliseconds(long startedAt) =>
        Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 3);
}
