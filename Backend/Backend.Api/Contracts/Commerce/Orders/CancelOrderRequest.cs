using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Contracts.Commerce.Orders;

public sealed record CancelOrderRequest
{
    [MaxLength(500)]
    public string? Reason { get; init; }
}
