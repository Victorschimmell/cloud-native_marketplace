namespace Backend.Api.Contracts.Commerce.Orders;

public sealed record UpdateOrderStatusRequest
{
    public required OrderStatus Status { get; init; }
}
