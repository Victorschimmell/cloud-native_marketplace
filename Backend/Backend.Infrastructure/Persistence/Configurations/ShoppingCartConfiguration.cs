using Backend.Domain.Entities.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("shopping_carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.HasOne(c => c.RecoveredFromCart)
            .WithMany(c => c.RecoveredCarts)
            .HasForeignKey(c => c.RecoveredFromCartId)
            .IsRequired(false);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId);

        builder.HasMany(c => c.OrdersPlacedFromCart)
            .WithOne(o => o.PlacedFromCart)
            .HasForeignKey(o => o.PlacedFromCartId)
            .IsRequired(false);
    }
}
