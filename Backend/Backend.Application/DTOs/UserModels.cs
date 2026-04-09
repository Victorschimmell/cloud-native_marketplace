using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

public sealed record CustomerDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Phone,
    Guid? DefaultAddressId);

public sealed record SellerDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string RegistrationNumber,
    string PayoutInformation,
    Guid? DefaultAddressId,
    VerificationStatus VerificationStatus,
    DateTimeOffset? VerifiedAtUtc);

public sealed record UserAccountDto(
    Guid Id,
    string Email,
    bool IsAdmin,
    bool IsBlocked,
    AccountStatus AccountStatus,
    DateTimeOffset? LastLoginAtUtc);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthTokenDto(string AccessToken, DateTimeOffset ExpiresAtUtc, string? RefreshToken = null);

public sealed record AuthenticationResponse(UserAccountDto User, AuthTokenDto Token);

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone,
    Guid? DefaultAddressId);

public sealed record RegisterSellerRequest(
    string Email,
    string Password,
    string BusinessName,
    string RegistrationNumber,
    string PayoutInformation,
    Guid? DefaultAddressId);

public sealed record RegistrationResponse(
    UserAccountDto User,
    CustomerDto? Customer,
    SellerDto? Seller);

public sealed record SellerVerificationRequestDto(
    Guid Id,
    Guid SellerId,
    DateTimeOffset SubmittedAtUtc,
    SellerVerificationRequestStatus Status,
    string BusinessNameSnapshot,
    string RegistrationNumberSnapshot,
    string SubmittedDetails,
    string? ReviewNotes,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAtUtc,
    string? RejectionReason);

public sealed record SubmitSellerVerificationRequest(Guid SellerId, string SubmittedDetails);

public sealed record VerifySellerRequest(
    Guid VerificationRequestId,
    Guid SellerId,
    bool Approve,
    string? ReviewNotes,
    string? RejectionReason);

public sealed record SellerVerificationResponse(
    SellerVerificationRequestDto Request,
    SellerDto Seller);
