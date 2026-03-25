using Backend.Domain.Base;

namespace Backend.Domain.Entities.Catalog;

public sealed class Product : AuditableEntity<Guid>
{
    public Product()
    {
        Id = Guid.NewGuid();
    }

    public Guid CategoryId { get; set; }
    public required string ProductName { get; set; }
    public required string Description { get; set; }
    public int ProductNameLength { get; set; }
    public int ProductDescriptionLength { get; set; }
    public int ProductPhotosQty { get; set; }
    public int ProductWeightG { get; set; }
    public int ProductLengthCm { get; set; }
    public int ProductHeightCm { get; set; }
    public int ProductWidthCm { get; set; }
    public string? OlistProductId { get; set; }

    public ProductCategory? Category { get; set; }
    public ICollection<ProductListing> Listings { get; } = [];
    public ICollection<Orders.OrderItem> OrderItems { get; } = [];
}
