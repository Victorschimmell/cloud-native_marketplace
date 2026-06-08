using Backend.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("number_sequence");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SequenceKey).HasMaxLength(100).IsRequired();

        builder.HasIndex(s => s.SequenceKey).IsUnique();

        builder.Property(s => s.LastValue).IsRequired();
    }
}
