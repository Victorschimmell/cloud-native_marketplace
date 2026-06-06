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
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public SellerVerificationService(
        ISellerVerificationRequestRepository verificationRequestRepository,
        ISellerRepository sellerRepository,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(verificationRequestRepository);
        ArgumentNullException.ThrowIfNull(sellerRepository);
        ArgumentNullException.ThrowIfNull(currentUserProvider);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _verificationRequestRepository = verificationRequestRepository;
        _sellerRepository = sellerRepository;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SellerVerificationResponse>> VerifySellerAsync(VerifySellerRequest request, CancellationToken cancellationToken = default)
    {
        var attemptedAction = request.Approve ? AuditActionType.Approve : AuditActionType.Reject;

        if (!_currentUserProvider.IsAdmin)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: request.VerificationRequestId.ToString(),
                Outcome: AuditOutcome.Forbidden,
                Details: "Non-admin attempted to review seller verification request."
            ), cancellationToken);

            return Result<SellerVerificationResponse>.Forbidden("Only admins can review seller verification requests.");
        }

        var verificationRequest = await _verificationRequestRepository.GetByIdAsync(request.VerificationRequestId, cancellationToken);
        if (verificationRequest is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: request.VerificationRequestId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Verification review failed: request not found."
            ), cancellationToken);

            return Result<SellerVerificationResponse>.NotFound("Verification request not found.");
        }

        if (verificationRequest.SellerId != request.SellerId)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: verificationRequest.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: $"Verification review failed: request belongs to seller {verificationRequest.SellerId}, not {request.SellerId}."
            ), cancellationToken);

            return Result<SellerVerificationResponse>.ValidationFailure("Verification request does not belong to the specified seller.");
        }

        if (verificationRequest.Status is SellerVerificationRequestStatus.Approved or SellerVerificationRequestStatus.Rejected)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: verificationRequest.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: $"Verification review failed: request already {verificationRequest.Status}."
            ), cancellationToken);

            return Result<SellerVerificationResponse>.Conflict("Verification request has already been reviewed.");
        }

        var seller = await _sellerRepository.GetByIdAsync(verificationRequest.SellerId, cancellationToken);
        if (seller is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: verificationRequest.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Verification review failed: seller not found."
            ), cancellationToken);

            return Result<SellerVerificationResponse>.NotFound("Seller not found.");
        }

        if (!request.Approve && string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: attemptedAction,
                TargetEntityType: nameof(SellerVerificationRequest),
                TargetEntityId: verificationRequest.Id.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Verification rejection failed: rejection reason missing."
            ), cancellationToken);

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

        var auditDetails = request.Approve
            ? $"Seller {seller.Id} verification request {verificationRequest.Id} approved."
            : $"Seller {seller.Id} verification request {verificationRequest.Id} rejected. Reason: {request.RejectionReason}";

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: attemptedAction,
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

    public async Task<Result<PagedResult<SellerVerificationRequestDto>>> GetAllRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paginationError = PaginationRules.Validate(page, pageSize);
        if (paginationError is not null)
        {
            return Result<PagedResult<SellerVerificationRequestDto>>.ValidationFailure(paginationError);
        }

        var requests = await _verificationRequestRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await _verificationRequestRepository.GetTotalCountAsync(cancellationToken);

        var dtos = requests.Select(r => r.ToSellerVerificationRequestDto()).ToList();
        return Result<PagedResult<SellerVerificationRequestDto>>.Success(
            new PagedResult<SellerVerificationRequestDto>(dtos, page, pageSize, totalCount));
    }
}
