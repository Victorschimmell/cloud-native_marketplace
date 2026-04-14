using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.Carts;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceGuardBehaviorTests
{
    [Fact]
    public async Task ProductService_GetByIdAsync_Fails_For_Empty_Id()
    {
        var service = new ProductService(new FakeProductRepository());

        var result = await service.GetByIdAsync(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("Product id is required.", result.Error);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task CustomerService_GetByIdAsync_Fails_With_NotFound_When_Customer_Does_Not_Exist()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Customer was not found.", result.Error);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task CartService_AddItemAsync_Fails_For_Invalid_Quantity()
    {
        var service = new CartService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeDateTimeProvider());

        var result = await service.AddItemAsync(new AddCartItemRequest(null, Guid.NewGuid(), null, Guid.NewGuid(), 0));

        Assert.True(result.IsFailure);
        Assert.Equal("Listing id is required and quantity must be greater than zero.", result.Error);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task CartService_GetCartAsync_Treats_Expired_Cart_As_NotFound()
    {
        var cartRepository = new FakeCartRepository
        {
            Cart = new ShoppingCart
            {
                UserId = Guid.NewGuid(),
                Status = Backend.Domain.Enums.CartStatus.Active,
                ExpiresAtUtc = new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero)
            }
        };
        var service = new CartService(cartRepository, new FakeProductListingRepository(), new FakeDateTimeProvider());

        var result = await service.GetCartAsync(new GetCartRequest(cartRepository.Cart.Id, null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("Cart was not found.", result.Error);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task AuthService_LoginAsync_Fails_For_Missing_Credentials()
    {
        var service = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider());

        var result = await service.LoginAsync(new LoginRequest("", ""));

        Assert.True(result.IsFailure);
        Assert.Equal("Email and password are required.", result.Error);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task AuthService_LoginAsync_Increments_Failed_Attempts_For_Invalid_Password()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = new UserAccount
            {
                Email = new EmailAddress("user@example.com"),
                PasswordHash = "hashed::correct-password",
                AccountStatus = AccountStatus.Active,
                FailedLoginAttempts = 2
            }
        };
        var service = new AuthService(userRepository, new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider());

        var result = await service.LoginAsync(new LoginRequest("user@example.com", "wrong-password"));

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.Unauthorized, result.FailureType);
        Assert.NotNull(userRepository.UserAccount);
        Assert.Equal(3, userRepository.UserAccount!.FailedLoginAttempts);
        Assert.Equal(1, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task AuthService_LoginAsync_Resets_Failed_Attempts_On_Success()
    {
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = new UserAccount
            {
                Email = new EmailAddress("user@example.com"),
                PasswordHash = "hashed::correct-password",
                AccountStatus = AccountStatus.Active,
                FailedLoginAttempts = 4
            }
        };
        var dateTimeProvider = new FakeDateTimeProvider();
        var service = new AuthService(userRepository, new FakePasswordHasher(), new FakeAuthTokenGenerator(), dateTimeProvider);

        var result = await service.LoginAsync(new LoginRequest("user@example.com", "correct-password"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(userRepository.UserAccount);
        Assert.Equal(0, userRepository.UserAccount!.FailedLoginAttempts);
        Assert.Equal(dateTimeProvider.UtcNow, userRepository.UserAccount.LastLoginAtUtc);
        Assert.Equal(1, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task SellerVerificationService_VerifySellerAsync_Updates_User_Account_Status()
    {
        var userAccount = new UserAccount
        {
            Email = new EmailAddress("seller@example.com"),
            PasswordHash = "hashed::password",
            AccountStatus = AccountStatus.PendingActivation
        };
        var seller = new Backend.Domain.Entities.IdentityAccess.Seller
        {
            UserId = userAccount.Id,
            BusinessName = "Seller A",
            RegistrationNumber = "REG-123",
            PayoutInformation = "IBAN"
        };
        var verificationRequest = new Backend.Domain.Entities.IdentityAccess.SellerVerificationRequest
        {
            SellerId = seller.Id,
            SubmittedAtUtc = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero),
            Status = SellerVerificationRequestStatus.Submitted,
            BusinessNameSnapshot = "Seller A",
            RegistrationNumberSnapshot = "REG-123",
            SubmittedDetails = "Verification details"
        };
        var verificationRepository = new FakeSellerVerificationRequestRepository
        {
            Request = verificationRequest
        };
        var sellerRepository = new FakeSellerRepository
        {
            Seller = seller
        };
        var userRepository = new FakeUserAccountRepository
        {
            UserAccount = userAccount
        };
        var service = new SellerVerificationService(
            verificationRepository,
            sellerRepository,
            userRepository,
            new FakeCurrentUserProvider(),
            new FakeDateTimeProvider());

        var result = await service.VerifySellerAsync(new VerifySellerRequest(verificationRequest.Id, seller.Id, true, "Approved", null));

        Assert.True(result.IsSuccess);
        Assert.NotNull(userRepository.UserAccount);
        Assert.Equal(AccountStatus.Active, userRepository.UserAccount!.AccountStatus);
        Assert.Equal(1, userRepository.UpdateCalls);
    }

    [Fact]
    public async Task CustomerService_GetCustomersAsync_Fails_For_Invalid_Paging()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        var result = await service.GetCustomersAsync(new PagedRequest(0, 10));

        Assert.True(result.IsFailure);
        Assert.Equal("Page and page size must be greater than zero.", result.Error);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }
}


