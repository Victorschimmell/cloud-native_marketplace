using Backend.Domain.Base;
using Backend.Domain.Entities.Location;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class Customer : AggregateRoot<Guid>
{
    public Customer()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public Guid? DefaultAddressId { get; set; }
    public string? OlistCustomerId { get; set; }
    public string? OlistCustomerUniqueId { get; set; }

    public UserAccount? UserAccount { get; set; }
    public Address? DefaultAddress { get; set; }
    public ICollection<Orders.Order> Orders { get; } = [];
}
