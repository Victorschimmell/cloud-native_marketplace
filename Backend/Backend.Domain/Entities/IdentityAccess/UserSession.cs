using Backend.Domain.Base;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class UserSession : Entity<Guid>
{
    public UserSession()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public required string IpAddress { get; set; }
    public required string UserAgent { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public bool IsActive { get; set; }

    public UserAccount? UserAccount { get; set; }
    public ICollection<Carts.ShoppingCart> GuestShoppingCarts { get; } = [];
}
