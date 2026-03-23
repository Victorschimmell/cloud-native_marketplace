using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Orders;

public sealed class OrderItem
{
    public Guid OrderId { get; set; }
    public int OrderItemId { get; set; }
    public Guid ListingId { get; set; }
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public int Quantity { get; set; }
    public Money UnitPrice { get; set; }
    public Money FreightValue { get; set; }
    public DateTimeOffset? ShippingLimitDateUtc { get; set; }

    public Order? Order { get; set; }
    public Catalog.ProductListing? Listing { get; set; }
    public Catalog.Product? Product { get; set; }
    public IdentityAccess.Seller? Seller { get; set; }
}
