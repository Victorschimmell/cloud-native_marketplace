using Backend.Domain.Base;

namespace Backend.Domain.Entities.Catalog;

public sealed class ProductCategory : Entity<Guid>
{
    public ProductCategory()
    {
        Id = Guid.NewGuid();
    }

    public required string CategoryNamePt { get; set; }
    public string? CategoryNameEn { get; set; }

    public ICollection<Product> Products { get; } = [];
}
