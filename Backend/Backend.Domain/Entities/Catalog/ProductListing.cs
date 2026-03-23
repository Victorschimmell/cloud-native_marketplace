using Backend.Domain.Base;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Catalog;

public sealed class ProductListing : AuditableEntity<Guid>
{
    public ProductListing()
    {
        Id = Guid.NewGuid();
    }

    public Guid SellerId { get; set; }
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public Money ListingPrice { get; set; }
    public int InventoryQuantity { get; set; }
    public ListingVisibilityStatus VisibilityStatus { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }

    public IdentityAccess.Seller? Seller { get; set; }
    public Product? Product { get; set; }
    public ICollection<Carts.CartItem> CartItems { get; } = [];
    public ICollection<Orders.OrderItem> OrderItems { get; } = [];
}
