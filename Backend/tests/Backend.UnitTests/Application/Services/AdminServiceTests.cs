using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class AdminServiceTests
{
    [Fact]
    public async Task BlockUserAsync_WhenCurrentUserIsNotAdmin_ReturnsForbidden()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("target@example.com")
        };
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            userRepository,
            currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false },
            auditLogService: auditLogService);

        var result = await service.BlockUserAsync(
            new AdminBlockUserRequest(userRepository.UserAccount.Id, "bad behavior"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.False(userRepository.UserAccount.IsBlocked);
        Assert.Equal(0, userRepository.UpdateCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task BlockUserAsync_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var userRepository = new FakeUserAccountRepository();
        var service = CreateService(userRepository);

        var result = await service.BlockUserAsync(
            new AdminBlockUserRequest(Guid.NewGuid(), null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task BlockUserAsync_WhenUserAlreadyBlocked_ReturnsConflict()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("blocked@example.com", isBlocked: true)
        };
        var service = CreateService(userRepository);

        var result = await service.BlockUserAsync(
            new AdminBlockUserRequest(userRepository.UserAccount.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(0, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task BlockUserAsync_WhenRequestIsValid_BlocksUserAndWritesAuditLog()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("active@example.com")
        };
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(userRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.BlockUserAsync(
            new AdminBlockUserRequest(userRepository.UserAccount.Id, "Terms violation"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.True(userRepository.UserAccount.IsBlocked);
        Assert.Equal(AccountStatus.Suspended, userRepository.UserAccount.AccountStatus);
        Assert.Equal("Block", result.Value!.Operation);
        Assert.Equal(1, userRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Contains("Terms violation", Assert.Single(auditLogService.Entries).Details);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenCurrentUserIsNotAdmin_ReturnsForbidden()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("blocked@example.com", isBlocked: true)
        };
        var service = CreateService(userRepository, currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.UnblockUserAsync(
            new AdminUnblockUserRequest(userRepository.UserAccount.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.True(userRepository.UserAccount.IsBlocked);
        Assert.Equal(0, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService(new FakeUserAccountRepository());

        var result = await service.UnblockUserAsync(
            new AdminUnblockUserRequest(Guid.NewGuid(), null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUserIsNotBlocked_ReturnsConflict()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("active@example.com")
        };
        var service = CreateService(userRepository);

        var result = await service.UnblockUserAsync(
            new AdminUnblockUserRequest(userRepository.UserAccount.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(0, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenRequestIsValid_UnblocksUserAndClearsLockoutState()
    {
        var user = CreateUser("blocked@example.com", isBlocked: true);
        user.FailedLoginAttempts = 5;
        user.LockedUntilUtc = DateTimeOffset.UtcNow.AddMinutes(10);
        var userRepository = new FakeUserAccountRepository { UserAccount = user };
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(userRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.UnblockUserAsync(
            new AdminUnblockUserRequest(user.Id, "Appeal accepted"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.False(user.IsBlocked);
        Assert.Equal(AccountStatus.Active, user.AccountStatus);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntilUtc);
        Assert.Equal("Unblock", result.Value!.Operation);
        Assert.Equal(1, userRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Contains("Appeal accepted", Assert.Single(auditLogService.Entries).Details);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenPaginationIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeUserAccountRepository());

        var result = await service.GetAuditLogsAsync(
            new GetAuditLogsRequest(null, null, null, Page: 0, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenEntityIdHasNoEntityType_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeUserAccountRepository());

        var result = await service.GetAuditLogsAsync(
            new GetAuditLogsRequest(null, null, "123", Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenFiltersMatch_ReturnsMappedLogs()
    {
        var actorId = Guid.NewGuid();
        var auditRepository = new FakeAuditLogRepository();
        auditRepository.AddedLogs.Add(new AuditLog
        {
            ActorUserId = actorId,
            ActorIpAddress = "127.0.0.1",
            ActionType = AuditActionType.Login,
            TargetEntityType = "UserAccount",
            TargetEntityId = "user-1",
            Outcome = AuditOutcome.Succeeded,
            Details = "Login succeeded",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        auditRepository.AddedLogs.Add(new AuditLog
        {
            ActorUserId = Guid.NewGuid(),
            ActionType = AuditActionType.Block,
            TargetEntityType = "UserAccount",
            TargetEntityId = "user-2",
            Outcome = AuditOutcome.Failed,
            Details = "Nope",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        var service = CreateService(new FakeUserAccountRepository(), auditRepository: auditRepository);

        var result = await service.GetAuditLogsAsync(
            new GetAuditLogsRequest(actorId, "UserAccount", "user-1", Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var log = Assert.Single(result.Value!.Items);
        Assert.Equal(actorId, log.ActorUserId);
        Assert.Equal("user-1", log.TargetEntityId);
        Assert.Equal(AuditActionType.Login, log.ActionType);
    }

    [Fact]
    public async Task GetUsersAsync_WhenCurrentUserIsNotAdmin_ReturnsForbidden()
    {
        var service = CreateService(new FakeUserAccountRepository(), currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.GetUsersAsync(
            new GetAdminUsersRequest(AdminUserRoleFilter.Any, AdminUserStatusFilter.Any),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
    }

    [Fact]
    public async Task GetUsersAsync_WhenPaginationIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeUserAccountRepository());

        var result = await service.GetUsersAsync(
            new GetAdminUsersRequest(AdminUserRoleFilter.Any, AdminUserStatusFilter.Any, Page: 1, PageSize: 0),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetUsersAsync_WhenUsersExist_MapsAdminSellerCustomerAndFallbackProfiles()
    {
        var userRepository = new FakeUserAccountRepository();
        userRepository.UserAccounts.Add(CreateUser("admin@example.com", isAdmin: true));
        userRepository.UserAccounts.Add(CreateSellerUser("seller@example.com", VerificationStatus.Pending));
        userRepository.UserAccounts.Add(CreateCustomerUser("customer@example.com", "Ada", "Lovelace"));
        userRepository.UserAccounts.Add(CreateUser("fallback@example.com"));
        var service = CreateService(userRepository);

        var result = await service.GetUsersAsync(
            new GetAdminUsersRequest(AdminUserRoleFilter.Any, AdminUserStatusFilter.Any, Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Items.Count);
        Assert.Contains(result.Value.Items, user => user.Role == "Admin" && user.DisplayName == "admin@example.com");
        Assert.Contains(result.Value.Items, user => user.Role == "Seller" && user.Status == "pending verification" && user.PendingVerificationRequestId is not null);
        Assert.Contains(result.Value.Items, user => user.Role == "Customer" && user.DisplayName == "Ada Lovelace");
        Assert.Contains(result.Value.Items, user => user.Email == "fallback@example.com" && user.DisplayName == "fallback@example.com");
    }

    [Fact]
    public async Task GetUsersAsync_WhenBlockedFilterIsProvided_ReturnsBlockedUsers()
    {
        var userRepository = new FakeUserAccountRepository();
        userRepository.UserAccounts.Add(CreateUser("active@example.com"));
        userRepository.UserAccounts.Add(CreateUser("blocked@example.com", isBlocked: true));
        var service = CreateService(userRepository);

        var result = await service.GetUsersAsync(
            new GetAdminUsersRequest(AdminUserRoleFilter.Any, AdminUserStatusFilter.Blocked, Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var user = Assert.Single(result.Value!.Items);
        Assert.Equal("blocked@example.com", user.Email);
        Assert.Equal("blocked", user.Status);
    }

    [Fact]
    public async Task GetPaymentsAsync_WhenCurrentUserIsNotAdmin_ReturnsForbidden()
    {
        var service = CreateService(new FakeUserAccountRepository(), currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.GetPaymentsAsync(
            new GetAdminPaymentsRequest(Currency: "BRL"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
    }

    [Fact]
    public async Task GetPaymentsAsync_WhenPaginationIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeUserAccountRepository());

        var result = await service.GetPaymentsAsync(
            new GetAdminPaymentsRequest(Page: 0, PageSize: 50, Currency: "BRL"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetPaymentsAsync_WhenCurrencyIsUnsupported_ReturnsValidationFailure()
    {
        var currencyConversion = new FakeCurrencyConversionService { ForceUnsupported = true };
        var service = CreateService(new FakeUserAccountRepository(), currencyConversionService: currencyConversion);

        var result = await service.GetPaymentsAsync(
            new GetAdminPaymentsRequest(Currency: "XYZ"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetPaymentsAsync_WhenPaymentsExist_MapsCustomerNameStatusDisplayIdAndCurrency()
    {
        var paymentRepository = new FakePaymentRepository();
        paymentRepository.Payments.Add(CreatePayment(
            orderNumber: "ORDER-123",
            firstName: "Grace",
            lastName: "Hopper",
            status: PaymentStatus.Paid,
            value: 100m,
            sequential: 2));
        paymentRepository.Payments.Add(CreatePayment(
            orderNumber: null,
            firstName: "",
            lastName: "",
            status: PaymentStatus.Failed,
            value: 50m,
            sequential: 1));
        var service = CreateService(new FakeUserAccountRepository(), paymentRepository: paymentRepository);

        var result = await service.GetPaymentsAsync(
            new GetAdminPaymentsRequest(Page: 1, PageSize: 20, Currency: "USD", Status: AdminPaymentStatusFilter.Any),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Items.Count);
        var paid = result.Value.Items.Single(payment => payment.Status == "completed");
        Assert.Equal("ORDER-123-2", paid.DisplayId);
        Assert.Equal("Grace Hopper", paid.CustomerName);
        Assert.Equal(50m, paid.Amount);
        Assert.Equal("USD", paid.CurrencyCode);

        var failed = result.Value.Items.Single(payment => payment.Status == "failed");
        Assert.Equal("Customer", failed.CustomerName);
    }

    private static AdminService CreateService(
        FakeUserAccountRepository userRepository,
        FakeAuditLogRepository? auditRepository = null,
        FakeAuditLogService? auditLogService = null,
        FakePaymentRepository? paymentRepository = null,
        FakeCurrencyConversionService? currencyConversionService = null,
        FakeCurrentUserProvider? currentUserProvider = null,
        FakeUnitOfWork? unitOfWork = null)
    {
        return new AdminService(
            userRepository,
            auditRepository ?? new FakeAuditLogRepository(),
            auditLogService ?? new FakeAuditLogService(),
            paymentRepository ?? new FakePaymentRepository(),
            currencyConversionService ?? new FakeCurrencyConversionService(),
            currentUserProvider ?? new FakeCurrentUserProvider(),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private static UserAccount CreateUser(string email, bool isAdmin = false, bool isBlocked = false)
    {
        return new UserAccount
        {
            Email = new EmailAddress(email),
            PasswordHash = "hashed",
            IsAdmin = isAdmin,
            IsBlocked = isBlocked,
            AccountStatus = isBlocked ? AccountStatus.Suspended : AccountStatus.Active,
            CreatedAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
    }

    private static UserAccount CreateCustomerUser(string email, string firstName, string lastName)
    {
        var user = CreateUser(email);
        var customer = new Customer
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Phone = "+4512345678"
        };

        user.CustomerProfile = customer;
        customer.UserAccount = user;
        return user;
    }

    private static UserAccount CreateSellerUser(string email, VerificationStatus verificationStatus)
    {
        var user = CreateUser(email);
        var seller = new Seller
        {
            UserId = user.Id,
            BusinessName = "Seller Co",
            RegistrationNumber = "REG-1",
            PayoutInformation = "Bank",
            VerificationStatus = verificationStatus
        };
        seller.VerificationRequests.Add(new SellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Status = SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = seller.BusinessName,
            RegistrationNumberSnapshot = seller.RegistrationNumber,
            SubmittedDetails = "Docs"
        });

        user.SellerProfile = seller;
        seller.UserAccount = user;
        return user;
    }

    private static OrderPayment CreatePayment(
        string? orderNumber,
        string firstName,
        string lastName,
        PaymentStatus status,
        decimal value,
        int sequential)
    {
        var customer = new Customer
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Phone = "+4512345678"
        };
        var order = new Order
        {
            CustomerId = customer.Id,
            ShippingAddressId = Guid.NewGuid(),
            OrderNumber = orderNumber ?? $"ORD-{Guid.NewGuid():N}",
            OrderPurchaseTimestampUtc = new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
            Customer = customer
        };
        var payment = new OrderPayment
        {
            OrderId = order.Id,
            PaymentSequential = sequential,
            PaymentStatus = status,
            PaymentValue = value,
            PaymentType = PaymentType.CreditCard,
            PaymentInstallments = 1,
            CurrencyId = Guid.NewGuid(),
            PaidAtUtc = status == PaymentStatus.Paid ? new DateTimeOffset(2026, 1, 4, 0, 0, 0, TimeSpan.Zero) : null,
            Order = order
        };

        order.Payments.Add(payment);
        return payment;
    }
}
