using Backend.Api.Attributes;

namespace Backend.Api.Contracts.User.SellerVerification;

public sealed record VerifySellerRequest
{
    [NotEmptyGuid]
    public required Guid VerificationRequestId { get; init; }
    public required bool Approve { get; init; }
    public string? ReviewNotes { get; init; }
    public string? RejectionReason { get; init; }
}
