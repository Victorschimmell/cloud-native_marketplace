using Backend.Domain.Base;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class Admin : AggregateRoot<Guid>
{
    public Admin()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public required string DisplayName { get; set; }

    public UserAccount? UserAccount { get; set; }
    public ICollection<UserBlock> AppliedBlocks { get; } = [];
    public ICollection<SellerVerificationRequest> ReviewedVerificationRequests { get; } = [];
}
