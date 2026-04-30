using System.Text.Json.Serialization;

namespace Backend.Api.Contracts.User.SellerVerification;

public enum VerificationStatus
{
    Unverified = 1,
    Pending = 2,
    Verified = 3,
    Rejected = 4
}
