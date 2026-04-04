using Backend.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActorIpAddress)
            .HasMaxLength(45);

        builder.Property(a => a.ActionType)
            .IsRequired();

        builder.Property(a => a.TargetEntityType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.TargetEntityId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Outcome)
            .IsRequired();

        builder.Property(a => a.Details)
            .IsRequired();

        builder.HasIndex(a => new { a.TargetEntityType, a.TargetEntityId });
        builder.HasIndex(a => a.ActorUserId);
    }
}
