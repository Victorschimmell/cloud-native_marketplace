using Backend.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.AddressLine1)
            .HasMaxLength(200);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(200);

        builder.Property(a => a.CountryCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.Latitude)
            .HasPrecision(10, 7);

        builder.Property(a => a.Longitude)
            .HasPrecision(10, 7);
    }
}
