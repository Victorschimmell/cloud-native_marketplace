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
        var unitOfWork = new FakeUnitOfWork();
        var service = new AuthService(userRepository, new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), unitOfWork);

        var result = await service.LoginAsync(new LoginRequest("customer@example.com", "Password123!"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal($"token-for-{userRepository.UserAccount.Id}", result.Value.Token.AccessToken);
        Assert.Equal(new DateTimeOffset(2026, 4, 9, 12, 0, 0, TimeSpan.Zero), userRepository.UserAccount.LastLoginAtUtc);
        Assert.Equal(1, userRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterCustomerAsync_HashesPasswordAndCreatesCustomerProfile()
    {
        var userRepository = new FakeUserAccountRepository();
        var customerRepository = new FakeCustomerRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new RegistrationService(userRepository, customerRepository, new FakeSellerRepository(), new FakePasswordHasher(), unitOfWork);

        var result = await service.RegisterCustomerAsync(
            new RegisterCustomerRequest("NEW@EXAMPLE.COM", "Password123!", " Ada ", " Lovelace ", "+4512345678", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(userRepository.UserAccount);
        Assert.Equal("new@example.com", userRepository.UserAccount.Email.Value);
        Assert.Equal("hashed::Password123!", userRepository.UserAccount.PasswordHash);
        Assert.NotNull(customerRepository.Customer);
        Assert.Equal(userRepository.UserAccount.Id, customerRepository.Customer.UserId);
        Assert.Equal("Ada", customerRepository.Customer.FirstName);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterSellerAsync_CreatesSellerProfileWithPendingVerification()
    {
        var userRepository = new FakeUserAccountRepository();
        var sellerRepository = new FakeSellerRepository();
        var service = new RegistrationService(userRepository, new FakeCustomerRepository(), sellerRepository, new FakePasswordHasher(), new FakeUnitOfWork());

        var result = await service.RegisterSellerAsync(
            new RegisterSellerRequest("seller@example.com", "Password123!", "Shop ApS", "DK-123", "IBAN", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(sellerRepository.Seller);
        Assert.Equal(userRepository.UserAccount!.Id, sellerRepository.Seller.UserId);
        Assert.Equal(VerificationStatus.Pending, sellerRepository.Seller.VerificationStatus);
    }

    private static UserAccount CreateUser(string email, string passwordHash) =>
        new()
        {
            Email = new EmailAddress(email),
            PasswordHash = passwordHash,
            AccountStatus = AccountStatus.Active
        };
}
