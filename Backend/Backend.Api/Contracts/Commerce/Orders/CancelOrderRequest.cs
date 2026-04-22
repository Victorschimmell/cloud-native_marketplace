namespace Backend.Api.Contracts.Commerce.Orders;

public sealed record CancelOrderRequest
{
    public string? Reason { get; init; }
}
