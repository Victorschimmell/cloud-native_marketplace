using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class AuthRegistrationCartServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUpdatesLastLogin()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = CreateUser("customer@example.com", "hashed::Password123!")
        };
        var customerRepository = new FakeCustomerRepository();
        await customerRepository.AddAsync(new Customer
        {
            UserId = userRepository.UserAccount.Id,
            FirstName = "Ada",
            LastName = "Lovelace",
            Phone = "+4512345678"
        }, TestContext.Current.CancellationToken);
        var unitOfWork = new FakeUnitOfWork();
        var service = new AuthService(userRepository, customerRepository, new FakeSellerRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeAuditLogService(), unitOfWork);

        var result = await service.LoginAsync(new LoginRequest("customer@example.com", "Password123!"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal($"token-for-{userRepository.UserAccount.Id}", result.Value.Token.AccessToken);
        Assert.NotNull(result.Value.Customer);
        Assert.Equal(userRepository.UserAccount.Id, result.Value.Customer.UserId);
        Assert.Equal(new DateTimeOffset(2026, 4, 9, 12, 0, 0, TimeSpan.Zero), userRepository.UserAccount.LastLoginAtUtc);
        Assert.Equal(1, userRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task LoginAsync_WithBlockedAccount_ReturnsGenericUnauthorizedFailure()
    {
        var user = CreateUser("blocked@example.com", "hashed::Password123!");
        user.IsBlocked = true;
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = user
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = new AuthService(userRepository, new FakeCustomerRepository(), new FakeSellerRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeAuditLogService(), unitOfWork);

        var result = await service.LoginAsync(new LoginRequest("blocked@example.com", "Password123!"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.Unauthorized, result.FailureType);
        Assert.Equal("Invalid email or password.", result.Error);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterCustomerAsync_HashesPasswordAndCreatesCustomerProfile()
    {
        var userRepository = new FakeUserAccountRepository();
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new RegistrationService(
            userRepository,
            customerRepository,
            new FakeSellerRepository(),
            new FakeSellerVerificationRequestRepository(),
            new FakePasswordHasher(),
            new FakeAuthTokenGenerator(),
            new FakeDateTimeProvider(),
            new FakeAuditLogService(),
            unitOfWork);

        var result = await service.RegisterCustomerAsync(
            new RegisterCustomerRequest("NEW@EXAMPLE.COM", "Password123!", " Ada ", " Lovelace ", "+4512345678", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(userRepository.UserAccount);
        Assert.Equal("new@example.com", userRepository.UserAccount.Email.Value);
        Assert.Equal("hashed::Password123!", userRepository.UserAccount.PasswordHash);
        Assert.NotNull(result.Value!.Token);
        Assert.NotNull(customerRepository.Customer);
        Assert.Equal(userRepository.UserAccount.Id, customerRepository.Customer.UserId);
        Assert.Equal("Ada", customerRepository.Customer.FirstName);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WhenEmailRaceViolatesUniqueConstraint_ReturnsConflict()
    {
        var unitOfWork = new FakeUnitOfWork
        {
            ExceptionToThrow = new UniqueConstraintViolationException(
                UniqueConstraintTarget.UserAccountEmail,
                "IX_user_account_Email",
                new InvalidOperationException())
        };
        var service = new RegistrationService(
            new FakeUserAccountRepository(),
            new FakeCustomerRepository(),
            new FakeSellerRepository(),
            new FakeSellerVerificationRequestRepository(),
            new FakePasswordHasher(),
            new FakeAuthTokenGenerator(),
            new FakeDateTimeProvider(),
            new FakeAuditLogService(),
            unitOfWork);

        var result = await service.RegisterCustomerAsync(
            new RegisterCustomerRequest("race@example.com", "Password123!", "Ada", "Lovelace", "+4512345678", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal("An account with this email address already exists.", result.Error);
    }

    [Fact]
    public async Task RegisterSellerAsync_CreatesSellerProfileWithPendingVerification()
    {
        var userRepository = new FakeUserAccountRepository();
        var sellerRepository = new FakeSellerRepository();
        var verificationRequestRepository = new FakeSellerVerificationRequestRepository();
        var service = new RegistrationService(
            userRepository,
            new FakeCustomerRepository(),
            sellerRepository,
            verificationRequestRepository,
            new FakePasswordHasher(),
            new FakeAuthTokenGenerator(),
            new FakeDateTimeProvider(),
            new FakeAuditLogService(),
            new FakeUnitOfWork());

        var result = await service.RegisterSellerAsync(
            new RegisterSellerRequest("seller@example.com", "Password123!", "Shop ApS", "DK-123", "IBAN", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(sellerRepository.Seller);
        Assert.Equal(userRepository.UserAccount!.Id, sellerRepository.Seller.UserId);
        Assert.Equal(VerificationStatus.Pending, sellerRepository.Seller.VerificationStatus);
        Assert.NotNull(verificationRequestRepository.Request);
        Assert.Equal(sellerRepository.Seller.Id, verificationRequestRepository.Request.SellerId);
        Assert.Null(result.Value!.Token);
    }

    private static UserAccount CreateUser(string email, string passwordHash) =>
        new()
        {
            Email = new EmailAddress(email),
            PasswordHash = passwordHash,
            AccountStatus = AccountStatus.Active
        };
}
