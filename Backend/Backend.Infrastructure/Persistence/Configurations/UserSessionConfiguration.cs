using Backend.Domain.Entities.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasMany(s => s.GuestShoppingCarts)
            .WithOne(c => c.UserSession)
            .HasForeignKey(c => c.SessionId)
            .IsRequired(false);
    }
}
