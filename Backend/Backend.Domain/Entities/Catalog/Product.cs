using Backend.Domain.Base;
using Backend.Domain.ValueObjects;

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
    public int ProductWeightGrams { get; set; }
    public required ProductDimensions DimensionsCm { get; set; }
    public string? OlistProductId { get; set; }

    public ProductCategory? Category { get; set; }
    public ICollection<ProductListing> Listings { get; } = [];
    public ICollection<Orders.OrderItem> OrderItems { get; } = [];
}
