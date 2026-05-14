using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class OrderReviewConfiguration : IEntityTypeConfiguration<OrderReview>
{
    public void Configure(EntityTypeBuilder<OrderReview> builder)
    {
        builder.ToTable("order_review");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.OlistReviewId)
            .HasMaxLength(100);

        builder.HasIndex(r => r.OlistReviewId)
            .IsUnique();

        builder.HasIndex(r => new { r.OrderId, r.OrderItemId })
            .IsUnique()
            .HasFilter("\"OrderItemId\" IS NOT NULL");

        builder.HasIndex(r => new { r.CustomerId, r.ProductId })
            .IsUnique();

        builder.Property(r => r.ReviewScore)
            .IsRequired();

        builder.Property(r => r.ReviewCommentTitle)
            .HasMaxLength(200);

        builder.Property(r => r.ReviewCommentMessage)
            .HasMaxLength(2000);

        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.OrderItem)
            .WithMany(i => i.Reviews)
            .HasForeignKey(r => new { r.OrderId, r.OrderItemId })
            .HasPrincipalKey(i => new { i.OrderId, i.OrderItemId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
