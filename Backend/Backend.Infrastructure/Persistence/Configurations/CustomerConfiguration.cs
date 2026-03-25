using Backend.Domain.Entities.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.OlistCustomerId)
            .HasMaxLength(100);

        builder.Property(c => c.OlistCustomerUniqueId)
            .HasMaxLength(100);

        builder.HasOne(c => c.DefaultAddress)
            .WithMany(a => a.DefaultForCustomers)
            .HasForeignKey(c => c.DefaultAddressId)
            .IsRequired(false);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);
    }
}
