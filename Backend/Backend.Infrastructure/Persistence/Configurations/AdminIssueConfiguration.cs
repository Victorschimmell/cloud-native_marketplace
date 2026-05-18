using Backend.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class AdminIssueConfiguration : IEntityTypeConfiguration<AdminIssue>
{
    public void Configure(EntityTypeBuilder<AdminIssue> builder)
    {
        builder.ToTable("admin_issue");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(i => i.Type)
            .IsRequired();

        builder.Property(i => i.Priority)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired();

        builder.Property(i => i.Resolution)
            .HasMaxLength(4000);

        builder.HasOne(i => i.ReportedByUser)
            .WithMany()
            .HasForeignKey(i => i.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AssignedToUser)
            .WithMany()
            .HasForeignKey(i => i.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ResolvedByUser)
            .WithMany()
            .HasForeignKey(i => i.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Priority);
        builder.HasIndex(i => i.ReportedByUserId);
    }
}
