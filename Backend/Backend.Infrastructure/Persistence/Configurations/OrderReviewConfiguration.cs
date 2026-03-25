using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class OrderReviewConfiguration : IEntityTypeConfiguration<OrderReview>
{
    public void Configure(EntityTypeBuilder<OrderReview> builder)
    {
        builder.ToTable("order_reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewScore)
            .IsRequired();

        builder.Property(r => r.ReviewCommentTitle)
            .HasMaxLength(200);

        builder.Property(r => r.ReviewCommentMessage)
            .HasMaxLength(2000);
    }
}
