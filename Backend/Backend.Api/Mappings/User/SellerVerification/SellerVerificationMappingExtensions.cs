using Backend.Api.Contracts.User.Registration;
using Backend.Api.Contracts.User.SellerVerification;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.User.SellerVerification;

public static class SellerVerificationMappingExtensions
{
    public static App.SubmitSellerVerificationRequest ToApplicationRequest(this SellerVerificationRequest request, Guid sellerId) =>
        new(sellerId, request.SubmittedDetails);

    public static App.VerifySellerRequest ToApplicationRequest(this VerifySellerRequest request, Guid sellerId) =>
        new(request.VerificationRequestId, sellerId, request.Approve, request.ReviewNotes, request.RejectionReason);

    public static SellerVerificationRequestDetailsModel ToDetailsModel(this App.SellerVerificationRequestDto dto) =>
        new()
        {
            Id = dto.Id,
            SellerId = dto.SellerId,
            SubmittedAtUtc = dto.SubmittedAtUtc,
            Status = (SellerVerificationRequestStatus)dto.Status,
            BusinessNameSnapshot = dto.BusinessNameSnapshot,
            RegistrationNumberSnapshot = dto.RegistrationNumberSnapshot,
            SubmittedDetails = dto.SubmittedDetails,
            ReviewNotes = dto.ReviewNotes,
            ReviewedByUserId = dto.ReviewedByUserId,
            ReviewedAtUtc = dto.ReviewedAtUtc,
            RejectionReason = dto.RejectionReason
        };

    public static SellerVerificationRequestDetailsResponse ToDetailsResponse(this App.SellerVerificationRequestDto dto) =>
        new()
        {
            Id = dto.Id,
            SellerId = dto.SellerId,
            SubmittedAtUtc = dto.SubmittedAtUtc,
            Status = (SellerVerificationRequestStatus)dto.Status,
            BusinessNameSnapshot = dto.BusinessNameSnapshot,
            RegistrationNumberSnapshot = dto.RegistrationNumberSnapshot,
            SubmittedDetails = dto.SubmittedDetails,
            ReviewNotes = dto.ReviewNotes,
            ReviewedByUserId = dto.ReviewedByUserId,
            ReviewedAtUtc = dto.ReviewedAtUtc,
            RejectionReason = dto.RejectionReason
        };

    public static SellerModel ToModel(this App.SellerDto dto) =>
        new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            BusinessName = dto.BusinessName,
            RegistrationNumber = dto.RegistrationNumber,
            PayoutInformation = dto.PayoutInformation,
            DefaultAddressId = dto.DefaultAddressId,
            VerificationStatus = (VerificationStatus)dto.VerificationStatus,
            VerifiedAtUtc = dto.VerifiedAtUtc,
            OlistSellerId = dto.OlistSellerId
        };

    public static SellerVerificationSubmissionResponse ToSubmissionResponse(this App.SellerVerificationResponse response) =>
        new()
        {
            Request = response.Request.ToDetailsModel(),
            Seller = response.Seller.ToModel()
        };
}
