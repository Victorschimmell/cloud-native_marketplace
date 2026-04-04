using Backend.Domain.Base;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities.IdentityAccess;

public sealed class SellerVerificationRequest : Entity<Guid>
{
    public SellerVerificationRequest()
    {
        Id = Guid.NewGuid();
    }

    public Guid SellerId { get; set; }
    public DateTimeOffset SubmittedAtUtc { get; set; }
    public SellerVerificationRequestStatus Status { get; set; }
    public required string BusinessNameSnapshot { get; set; }
    public required string RegistrationNumberSnapshot { get; set; }
    public required string SubmittedDetails { get; set; }
    public string? ReviewNotes { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }

    public Seller? Seller { get; set; }
    public UserAccount? ReviewedByUser { get; set; }
}
