using Backend.Domain.Base;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Carts;

public sealed class CartItem : Entity<Guid>
{
    public CartItem()
    {
        Id = Guid.NewGuid();
    }

    public Guid CartId { get; set; }
    public Guid ListingId { get; set; }
    public int Quantity { get; set; }
    public Money UnitPriceAtAddition { get; set; }
    public DateTimeOffset AddedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ShoppingCart? Cart { get; set; }
    public Catalog.ProductListing? Listing { get; set; }
}
