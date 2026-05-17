using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("user_account");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => new EmailAddress(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.AccountStatus)
            .IsRequired();

        builder.HasOne(u => u.CustomerProfile)
            .WithOne(c => c.UserAccount)
            .HasForeignKey<Customer>(c => c.UserId);

        builder.HasOne(u => u.SellerProfile)
            .WithOne(s => s.UserAccount)
            .HasForeignKey<Seller>(s => s.UserId);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.UserAccount)
            .HasForeignKey(s => s.UserId);

        builder.HasMany(u => u.ShoppingCarts)
            .WithOne(c => c.UserAccount)
            .HasForeignKey(c => c.UserId);

        builder.HasMany(u => u.ReviewedVerificationRequests)
            .WithOne(r => r.ReviewedByUser)
            .HasForeignKey(r => r.ReviewedByUserId);

        builder.HasMany(u => u.AuditLogs)
            .WithOne(a => a.ActorUser)
            .HasForeignKey(a => a.ActorUserId);
    }
}
