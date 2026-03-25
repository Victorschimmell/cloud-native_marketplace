using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("order");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.OrderStatus)
            .IsRequired();

        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(o => o.FreightAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasOne(o => o.ShippingAddress)
            .WithMany(a => a.ShippingOrders)
            .HasForeignKey(o => o.ShippingAddressId);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId);

        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId);

        builder.HasMany(o => o.Reviews)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId);

        builder.HasMany(o => o.Shipments)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId);
    }
}
