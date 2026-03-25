using Backend.Domain.Entities.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.ToTable("seller");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BusinessName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.RegistrationNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.PayoutInformation)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.VerificationStatus)
            .IsRequired();

        builder.Property(s => s.OlistSellerId)
            .HasMaxLength(100);

        builder.HasOne(s => s.DefaultAddress)
            .WithMany(a => a.DefaultForSellers)
            .HasForeignKey(s => s.DefaultAddressId)
            .IsRequired(false);

        builder.HasMany(s => s.VerificationRequests)
            .WithOne(r => r.Seller)
            .HasForeignKey(r => r.SellerId);

        builder.HasMany(s => s.Listings)
            .WithOne(l => l.Seller)
            .HasForeignKey(l => l.SellerId);
    }
}
