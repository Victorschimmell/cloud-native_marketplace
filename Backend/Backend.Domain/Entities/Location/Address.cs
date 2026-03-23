using Backend.Domain.Base;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.Location;

public sealed class Address : AuditableEntity<Guid>
{
    public Address()
    {
        Id = Guid.NewGuid();
    }

    public required string PostalCode { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string CountryCode { get; set; }
    public GeoCoordinate? Coordinates { get; set; }

    public ICollection<IdentityAccess.Customer> DefaultForCustomers { get; } = [];
    public ICollection<IdentityAccess.Seller> DefaultForSellers { get; } = [];
    public ICollection<Orders.Order> ShippingOrders { get; } = [];
}
