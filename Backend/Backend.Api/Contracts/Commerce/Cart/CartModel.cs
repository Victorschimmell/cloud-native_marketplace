namespace Backend.Api.Contracts.Commerce.Cart;

public sealed record CartModel
{
    public required Guid Id { get; init; }
    public required Guid? UserId { get; init; }
    public required Guid? SessionId { get; init; }
    public required CartStatus Status { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }
    public required IReadOnlyList<CartItemModel> Items { get; init; }
}
