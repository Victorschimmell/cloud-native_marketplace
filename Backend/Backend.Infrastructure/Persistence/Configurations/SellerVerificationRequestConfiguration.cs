using Backend.Domain.Entities.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class SellerVerificationRequestConfiguration : IEntityTypeConfiguration<SellerVerificationRequest>
{
    public void Configure(EntityTypeBuilder<SellerVerificationRequest> builder)
    {
        builder.ToTable("seller_verification_request");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.BusinessNameSnapshot)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.RegistrationNumberSnapshot)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.SubmittedDetails)
            .IsRequired();

        builder.Property(r => r.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.Status)
            .IsRequired();
    }
}
