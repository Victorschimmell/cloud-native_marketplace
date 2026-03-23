using Backend.Domain.Base;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class UserBlock : AuditableEntity<Guid>
{
    public UserBlock()
    {
        Id = Guid.NewGuid();
    }

    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public Guid BlockedByAdminId { get; set; }
    public required string Reason { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public bool IsActive { get; set; }

    public UserAccount? UserAccount { get; set; }
    public Admin? BlockedByAdmin { get; set; }
}
