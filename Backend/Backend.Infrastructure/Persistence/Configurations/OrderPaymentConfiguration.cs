using Backend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.ToTable("order_payments");

        builder.HasKey(p => new { p.OrderId, p.PaymentSequential });

        builder.Property(p => p.PaymentType)
            .IsRequired();

        builder.Property(p => p.PaymentStatus)
            .IsRequired();

        builder.Property(p => p.PaymentValue)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(p => p.ExternalPaymentReference)
            .HasMaxLength(200);

        builder.HasOne(p => p.Currency)
            .WithMany(c => c.OrderPayments)
            .HasForeignKey(p => p.CurrencyId);
    }
}
