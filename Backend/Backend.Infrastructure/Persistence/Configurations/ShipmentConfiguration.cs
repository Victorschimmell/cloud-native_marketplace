using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CarrierName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(s => s.TrackingNumber);

        builder.Property(s => s.ShipmentStatus)
            .IsRequired();

        builder.HasOne(s => s.Seller)
            .WithMany(sel => sel.Shipments)
            .HasForeignKey(s => s.SellerId);
    }
}
