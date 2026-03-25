using Backend.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class ProductListingConfiguration : IEntityTypeConfiguration<ProductListing>
{
    public void Configure(EntityTypeBuilder<ProductListing> builder)
    {
        builder.ToTable("product_listings");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(l => l.Sku)
            .IsUnique();

        builder.Property(l => l.ListingPrice)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.VisibilityStatus)
            .IsRequired();

        builder.HasMany(l => l.CartItems)
            .WithOne(ci => ci.Listing)
            .HasForeignKey(ci => ci.ListingId);

        builder.HasMany(l => l.OrderItems)
            .WithOne(oi => oi.Listing)
            .HasForeignKey(oi => oi.ListingId);
    }
}
