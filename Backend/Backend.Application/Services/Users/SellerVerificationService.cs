using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public sealed class SellerVerificationService : ISellerVerificationService
{
    private readonly ISellerVerificationRequestRepository _verificationRequestRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SellerVerificationService(
        ISellerVerificationRequestRepository verificationRequestRepository,
        ISellerRepository sellerRepository,
        IUserAccountRepository userAccountRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(verificationRequestRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _verificationRequestRepository = verificationRequestRepository;
        _sellerRepository = sellerRepository;
        _userAccountRepository = userAccountRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SellerVerificationResponse>> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.SellerId == Guid.Empty || string.IsNullOrWhiteSpace(request.SubmittedDetails))
            {
                return Result<SellerVerificationResponse>.ValidationFailure("Seller id and submitted details are required.");
            }

            var seller = await _sellerRepository.GetByIdAsync(request.SellerId, cancellationToken);
            if (seller is null)
            {
                return Result<SellerVerificationResponse>.NotFound("Seller was not found.");
            }

            var verificationRequest = new SellerVerificationRequest
            {
                SellerId = seller.Id,
                SubmittedAtUtc = _dateTimeProvider.UtcNow,
                Status = SellerVerificationRequestStatus.Submitted,
                BusinessNameSnapshot = seller.BusinessName,
                RegistrationNumberSnapshot = seller.RegistrationNumber,
                SubmittedDetails = request.SubmittedDetails.Trim()
            };

            await _verificationRequestRepository.AddAsync(verificationRequest, cancellationToken);

            return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(verificationRequest.ToDto(), seller.ToDto()));
        }, "Unable to submit seller verification.");
    }

    public async Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.VerificationRequestId == Guid.Empty || request.SellerId == Guid.Empty)
            {
                return Result<SellerVerificationResponse>.ValidationFailure("Verification request id and seller id are required.");
            }

            var verificationRequest = await _verificationRequestRepository.GetByIdAsync(request.VerificationRequestId, cancellationToken);
            var seller = await _sellerRepository.GetByIdAsync(request.SellerId, cancellationToken);
            var userAccount = seller is null
                ? null
                : await _userAccountRepository.GetByIdAsync(seller.UserId, cancellationToken);

            if (verificationRequest is null || seller is null || userAccount is null)
            {
                return Result<SellerVerificationResponse>.NotFound("Seller verification request was not found.");
            }

            verificationRequest.Status = request.Approve ? SellerVerificationRequestStatus.Approved : SellerVerificationRequestStatus.Rejected;
            verificationRequest.ReviewNotes = request.ReviewNotes;
            verificationRequest.RejectionReason = request.Approve ? null : request.RejectionReason;
            verificationRequest.ReviewedAtUtc = _dateTimeProvider.UtcNow;
            verificationRequest.ReviewedByUserId = _currentUserProvider.UserId;

            seller.VerificationStatus = request.Approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
            seller.VerifiedAtUtc = request.Approve ? _dateTimeProvider.UtcNow : null;
            userAccount.AccountStatus = request.Approve ? AccountStatus.Active : AccountStatus.Suspended;

            await _verificationRequestRepository.UpdateAsync(verificationRequest, cancellationToken);
            await _sellerRepository.UpdateAsync(seller, cancellationToken);
            await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);

            return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(verificationRequest.ToDto(), seller.ToDto()));
        }, "Unable to verify seller.");
    }

    public async Task<Result<IReadOnlyList<SellerVerificationRequestDto>>> GetRequestsBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        if (sellerId == Guid.Empty)
        {
            return Result<IReadOnlyList<SellerVerificationRequestDto>>.ValidationFailure("Seller id is required.");
        }

        var requests = await _verificationRequestRepository.GetBySellerIdAsync(sellerId, cancellationToken);
        return Result<IReadOnlyList<SellerVerificationRequestDto>>.Success(requests.Select(static request => request.ToDto()).ToArray());
    }
}
