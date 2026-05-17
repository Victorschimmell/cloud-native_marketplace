using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Carts;

public sealed class ShoppingCart : AuditableEntity<Guid>
{
    public ShoppingCart()
    {
        Id = Guid.NewGuid();
    }

    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public CartStatus Status { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public Guid? RecoveredFromCartId { get; set; }

    public IdentityAccess.UserAccount? UserAccount { get; set; }
    public IdentityAccess.UserSession? UserSession { get; set; }
    public ShoppingCart? RecoveredFromCart { get; set; }
    public ICollection<ShoppingCart> RecoveredCarts { get; } = [];
    public ICollection<CartItem> Items { get; } = [];
    public ICollection<Orders.Order> OrdersPlacedFromCart { get; } = [];
}
