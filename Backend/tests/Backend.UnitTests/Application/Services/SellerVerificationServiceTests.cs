using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class SellerVerificationServiceTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task SubmitVerificationAsync_WhenSellerDoesNotExist_ReturnsNotFoundAndWritesAuditLog()
    {
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            new FakeSellerRepository(),
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            auditLogService: auditLogService);

        var result = await service.SubmitVerificationAsync(
            new SubmitSellerVerificationRequest(Guid.NewGuid(), "documents attached"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task SubmitVerificationAsync_WhenSellerExists_CreatesRequestAndSetsSellerPending()
    {
        var seller = CreateSeller(CurrentUserId, verificationStatus: VerificationStatus.Rejected);
        var sellerRepository = new FakeSellerRepository { Seller = seller };
        var requestRepository = new FakeSellerVerificationRequestRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            sellerRepository,
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            unitOfWork: unitOfWork,
            auditLogService: auditLogService);

        var result = await service.SubmitVerificationAsync(
            new SubmitSellerVerificationRequest(seller.Id, "updated documents"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(SellerVerificationRequestStatus.Submitted, result.Value!.Request.Status);
        Assert.Equal(VerificationStatus.Pending, seller.VerificationStatus);
        Assert.Equal("updated documents", requestRepository.Request!.SubmittedDetails);
        Assert.Equal(1, requestRepository.AddCalls);
        Assert.Equal(1, sellerRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task SubmitVerificationAsync_WhenDetailsAreBlank_ReturnsValidationFailure()
    {
        var seller = CreateSeller(CurrentUserId);
        var requestRepository = new FakeSellerVerificationRequestRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            unitOfWork: unitOfWork);

        var result = await service.SubmitVerificationAsync(
            new SubmitSellerVerificationRequest(seller.Id, " "),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(0, requestRepository.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task SubmitVerificationAsync_WhenDetailsAreTooLong_ReturnsValidationFailure()
    {
        var seller = CreateSeller(CurrentUserId);
        var requestRepository = new FakeSellerVerificationRequestRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            unitOfWork: unitOfWork);

        var result = await service.SubmitVerificationAsync(
            new SubmitSellerVerificationRequest(seller.Id, new string('a', 4001)),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(0, requestRepository.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task SubmitVerificationAsync_WhenAuthenticatedUserDoesNotOwnSeller_ReturnsForbidden()
    {
        var seller = CreateSeller(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var requestRepository = new FakeSellerVerificationRequestRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            unitOfWork: unitOfWork,
            auditLogService: auditLogService);

        var result = await service.SubmitVerificationAsync(
            new SubmitSellerVerificationRequest(seller.Id, "documents attached"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Equal(0, requestRepository.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenCurrentUserIsNotAdmin_ReturnsForbiddenAndWritesAuditLog()
    {
        var seller = CreateSeller();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false, UserId = CurrentUserId },
            auditLogService: auditLogService);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(Guid.NewGuid(), seller.Id, true, null, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenRequestDoesNotExist_ReturnsNotFound()
    {
        var seller = CreateSeller();
        var service = CreateService(new FakeSellerRepository { Seller = seller });

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(Guid.NewGuid(), seller.Id, true, null, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenRequestBelongsToDifferentSeller_ReturnsValidationFailure()
    {
        var seller = CreateSeller();
        var verificationRequest = CreateVerificationRequest(seller.Id);
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var service = CreateService(new FakeSellerRepository { Seller = seller }, requestRepository);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, Guid.NewGuid(), true, null, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Theory]
    [InlineData(SellerVerificationRequestStatus.Approved)]
    [InlineData(SellerVerificationRequestStatus.Rejected)]
    public async Task VerifySellerAsync_WhenRequestWasAlreadyReviewed_ReturnsConflict(SellerVerificationRequestStatus status)
    {
        var seller = CreateSeller();
        var verificationRequest = CreateVerificationRequest(seller.Id, status);
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var service = CreateService(new FakeSellerRepository { Seller = seller }, requestRepository);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, seller.Id, true, null, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenSellerDoesNotExist_ReturnsNotFound()
    {
        var verificationRequest = CreateVerificationRequest(Guid.NewGuid());
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var service = CreateService(new FakeSellerRepository(), requestRepository);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, verificationRequest.SellerId, true, null, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenRejectingWithoutReason_ReturnsValidationFailure()
    {
        var seller = CreateSeller();
        var verificationRequest = CreateVerificationRequest(seller.Id);
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var service = CreateService(new FakeSellerRepository { Seller = seller }, requestRepository);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, seller.Id, false, "Not enough info", " "),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(SellerVerificationRequestStatus.Submitted, verificationRequest.Status);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenApprovingRequest_VerifiesSellerAndWritesAuditLogs()
    {
        var seller = CreateSeller(verificationStatus: VerificationStatus.Pending);
        var verificationRequest = CreateVerificationRequest(seller.Id);
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var sellerRepository = new FakeSellerRepository { Seller = seller };
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(sellerRepository, requestRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, seller.Id, true, "Looks good", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(SellerVerificationRequestStatus.Approved, verificationRequest.Status);
        Assert.Equal(VerificationStatus.Verified, seller.VerificationStatus);
        Assert.NotNull(seller.VerifiedAtUtc);
        Assert.Equal(CurrentUserId, verificationRequest.ReviewedByUserId);
        Assert.Equal("Looks good", verificationRequest.ReviewNotes);
        Assert.Null(verificationRequest.RejectionReason);
        Assert.Equal(1, requestRepository.UpdateCalls);
        Assert.Equal(1, sellerRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal(2, auditLogService.Entries.Count);
    }

    [Fact]
    public async Task VerifySellerAsync_WhenRejectingRequest_RejectsSellerAndStoresReason()
    {
        var seller = CreateSeller(verificationStatus: VerificationStatus.Pending);
        seller.VerifiedAtUtc = new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero);
        var verificationRequest = CreateVerificationRequest(seller.Id);
        var requestRepository = new FakeSellerVerificationRequestRepository { Request = verificationRequest };
        var service = CreateService(new FakeSellerRepository { Seller = seller }, requestRepository);

        var result = await service.VerifySellerAsync(
            new VerifySellerRequest(verificationRequest.Id, seller.Id, false, "Missing registry", "Registry document is unreadable"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(SellerVerificationRequestStatus.Rejected, verificationRequest.Status);
        Assert.Equal(VerificationStatus.Rejected, seller.VerificationStatus);
        Assert.Null(seller.VerifiedAtUtc);
        Assert.Equal("Registry document is unreadable", verificationRequest.RejectionReason);
    }

    [Fact]
    public async Task GetRequestsBySellerAsync_WhenSellerDoesNotExist_ReturnsNotFoundAndWritesAuditLog()
    {
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(new FakeSellerRepository(), auditLogService: auditLogService);

        var result = await service.GetRequestsBySellerAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task GetRequestsBySellerAsync_WhenSellerExists_ReturnsSellerRequests()
    {
        var seller = CreateSeller(CurrentUserId);
        var requestRepository = new FakeSellerVerificationRequestRepository();
        requestRepository.Requests.Add(CreateVerificationRequest(seller.Id));
        requestRepository.Requests.Add(CreateVerificationRequest(Guid.NewGuid()));
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false });

        var result = await service.GetRequestsBySellerAsync(seller.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetRequestsBySellerAsync_WhenAuthenticatedUserDoesNotOwnSeller_ReturnsForbidden()
    {
        var seller = CreateSeller(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var requestRepository = new FakeSellerVerificationRequestRepository();
        requestRepository.Requests.Add(CreateVerificationRequest(seller.Id));
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            new FakeSellerRepository { Seller = seller },
            requestRepository,
            currentUserProvider: new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = false },
            auditLogService: auditLogService);

        var result = await service.GetRequestsBySellerAsync(seller.Id, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task GetAllRequestsAsync_WhenPaginationIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeSellerRepository());

        var result = await service.GetAllRequestsAsync(0, 10, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetAllRequestsAsync_WhenPaginationIsValid_ReturnsPagedRequests()
    {
        var firstSellerId = Guid.NewGuid();
        var requestRepository = new FakeSellerVerificationRequestRepository();
        requestRepository.Requests.Add(CreateVerificationRequest(firstSellerId));
        requestRepository.Requests.Add(CreateVerificationRequest(Guid.NewGuid()));
        var service = CreateService(new FakeSellerRepository(), requestRepository);

        var result = await service.GetAllRequestsAsync(1, 1, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(2, result.Value.TotalCount);
    }

    private static SellerVerificationService CreateService(
        FakeSellerRepository sellerRepository,
        FakeSellerVerificationRequestRepository? requestRepository = null,
        FakeUserAccountRepository? userAccountRepository = null,
        FakeCurrentUserProvider? currentUserProvider = null,
        FakeDateTimeProvider? dateTimeProvider = null,
        FakeAuditLogService? auditLogService = null,
        FakeUnitOfWork? unitOfWork = null) =>
        new(
            requestRepository ?? new FakeSellerVerificationRequestRepository(),
            sellerRepository,
            userAccountRepository ?? new FakeUserAccountRepository(),
            currentUserProvider ?? new FakeCurrentUserProvider { UserId = CurrentUserId, IsAdmin = true },
            dateTimeProvider ?? new FakeDateTimeProvider(),
            auditLogService ?? new FakeAuditLogService(),
            unitOfWork ?? new FakeUnitOfWork());

    private static Seller CreateSeller(VerificationStatus verificationStatus = VerificationStatus.Pending) =>
        CreateSeller(Guid.Parse("22222222-2222-2222-2222-222222222222"), verificationStatus);

    private static Seller CreateSeller(Guid userId, VerificationStatus verificationStatus = VerificationStatus.Pending) =>
        new()
        {
            UserId = userId,
            BusinessName = "Coffee Seller",
            RegistrationNumber = "123456",
            PayoutInformation = "bank",
            VerificationStatus = verificationStatus
        };

    private static SellerVerificationRequest CreateVerificationRequest(
        Guid sellerId,
        SellerVerificationRequestStatus status = SellerVerificationRequestStatus.Submitted) =>
        new()
        {
            SellerId = sellerId,
            SubmittedAtUtc = new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero),
            Status = status,
            BusinessNameSnapshot = "Coffee Seller",
            RegistrationNumberSnapshot = "123456",
            SubmittedDetails = "documents attached"
        };
}
