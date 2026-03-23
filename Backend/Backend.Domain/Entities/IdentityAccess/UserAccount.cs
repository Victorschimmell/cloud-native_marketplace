using Backend.Domain.Base;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class UserAccount : AggregateRoot<Guid>
{
    public UserAccount()
    {
        Id = Guid.NewGuid();
    }

    public required EmailAddress Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockedUntilUtc { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public Customer? CustomerProfile { get; set; }
    public Seller? SellerProfile { get; set; }
    public Admin? AdminProfile { get; set; }
    public ICollection<UserSession> Sessions { get; } = [];
    public ICollection<UserBlock> Blocks { get; } = [];
    public ICollection<Carts.ShoppingCart> ShoppingCarts { get; } = [];
    public ICollection<Operations.Notification> Notifications { get; } = [];
    public ICollection<Operations.AuditLog> AuditLogs { get; } = [];
}
