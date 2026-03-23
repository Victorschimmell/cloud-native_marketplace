using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.Operations;

public sealed class Notification : Entity<Guid>
{
    public Notification()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public NotificationType NotificationType { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }

    public IdentityAccess.UserAccount? UserAccount { get; set; }
    public Orders.Order? Order { get; set; }
}
