using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_item");

        builder.HasKey(i => new { i.OrderId, i.OrderItemId });

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(i => i.FreightValue)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasOne(i => i.Seller)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(i => i.SellerId);
    }
}
