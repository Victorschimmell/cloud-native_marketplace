using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Models;
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
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public SellerVerificationService(
        ISellerVerificationRequestRepository verificationRequestRepository,
        ISellerRepository sellerRepository,
        IUserAccountRepository userAccountRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(verificationRequestRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(userAccountRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _verificationRequestRepository = verificationRequestRepository;
        _sellerRepository = sellerRepository;
        _userAccountRepository = userAccountRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SellerVerificationResponse>> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var seller = await _sellerRepository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerVerificationResponse>.NotFound("Seller not found.");
        }

        var verificationRequest = new SellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Status = SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = request.SubmittedDetails
        };

        seller.VerificationStatus = VerificationStatus.Pending;

        await _verificationRequestRepository.AddAsync(verificationRequest, cancellationToken);
        await _sellerRepository.UpdateAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Created,
            TargetEntityType: nameof(SellerVerificationRequest),
            TargetEntityId: verificationRequest.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Verification request submitted for seller {seller.Id}"
        ), cancellationToken);

        return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(
            verificationRequest.ToSellerVerificationRequestDto(),
            seller.ToSellerDto()));
    }

    public async Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserProvider.IsAdmin)
        {
            return Result<SellerVerificationResponse>.Forbidden("Only admins can review seller verification requests.");
        }

        var verificationRequest = await _verificationRequestRepository.GetByIdAsync(request.VerificationRequestId, cancellationToken);
        if (verificationRequest is null)
        {
            return Result<SellerVerificationResponse>.NotFound("Verification request not found.");
        }

        if (verificationRequest.SellerId != request.SellerId)
        {
            return Result<SellerVerificationResponse>.ValidationFailure("Verification request does not belong to the specified seller.");
        }

        if (verificationRequest.Status is SellerVerificationRequestStatus.Approved or SellerVerificationRequestStatus.Rejected)
        {
            return Result<SellerVerificationResponse>.Conflict("Verification request has already been reviewed.");
        }

        var seller = await _sellerRepository.GetByIdAsync(verificationRequest.SellerId, cancellationToken);
        if (seller is null)
        {
            return Result<SellerVerificationResponse>.NotFound("Seller not found.");
        }

        if (!request.Approve && string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            return Result<SellerVerificationResponse>.ValidationFailure("RejectionReason is required when rejecting a verification request.");
        }

        var now = _dateTimeProvider.UtcNow;
        verificationRequest.ReviewedByUserId = _currentUserProvider.UserId;
        verificationRequest.ReviewedAtUtc = now;
        verificationRequest.ReviewNotes = request.ReviewNotes;

        if (request.Approve)
        {
            verificationRequest.Status = SellerVerificationRequestStatus.Approved;
            verificationRequest.RejectionReason = null;
            seller.VerificationStatus = VerificationStatus.Verified;
            seller.VerifiedAtUtc = now;
        }
        else
        {
            verificationRequest.Status = SellerVerificationRequestStatus.Rejected;
            verificationRequest.RejectionReason = request.RejectionReason;
            seller.VerificationStatus = VerificationStatus.Rejected;
            seller.VerifiedAtUtc = null;
        }

        await _verificationRequestRepository.UpdateAsync(verificationRequest, cancellationToken);
        await _sellerRepository.UpdateAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var auditAction = request.Approve ? AuditActionType.Approve : AuditActionType.Reject;
        var auditDetails = request.Approve
            ? $"Seller {seller.Id} verification request {verificationRequest.Id} approved."
            : $"Seller {seller.Id} verification request {verificationRequest.Id} rejected. Reason: {request.RejectionReason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: auditAction,
            TargetEntityType: nameof(SellerVerificationRequest),
            TargetEntityId: verificationRequest.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: auditDetails
        ), cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Updated,
            TargetEntityType: nameof(Seller),
            TargetEntityId: seller.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Seller verification status set to {seller.VerificationStatus}."
        ), cancellationToken);

        return Result<SellerVerificationResponse>.Success(new SellerVerificationResponse(
            verificationRequest.ToSellerVerificationRequestDto(),
            seller.ToSellerDto()));
    }

    public async Task<Result<IReadOnlyList<SellerVerificationRequestDto>>> GetRequestsBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var seller = await _sellerRepository.GetByIdAsync(sellerId, cancellationToken);
        if (seller is null)
        {
            return Result<IReadOnlyList<SellerVerificationRequestDto>>.NotFound("Seller not found.");
        }

        var requests = await _verificationRequestRepository.GetBySellerIdAsync(sellerId, cancellationToken);
        var dtos = requests.Select(r => r.ToSellerVerificationRequestDto()).ToList();
        return Result<IReadOnlyList<SellerVerificationRequestDto>>.Success(dtos);
    }

    public async Task<Result<PagedResult<SellerVerificationRequestDto>>> GetAllRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return Result<PagedResult<SellerVerificationRequestDto>>.ValidationFailure("Page and PageSize must be greater than 0.");
        }

        var requests = await _verificationRequestRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await _verificationRequestRepository.GetTotalCountAsync(cancellationToken);

        var dtos = requests.Select(r => r.ToSellerVerificationRequestDto()).ToList();
        return Result<PagedResult<SellerVerificationRequestDto>>.Success(
            new PagedResult<SellerVerificationRequestDto>(dtos, page, pageSize, totalCount));
    }
}
