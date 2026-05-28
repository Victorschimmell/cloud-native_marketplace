using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceSkeletonTests
{
    [Fact]
    public async Task AuthService_LoginAsync_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        var service = new AuthService(new FakeUserAccountRepository(), new FakeCustomerRepository(), new FakeSellerRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeAuditLogService(), new FakeUnitOfWork());

        var result = await service.LoginAsync(new LoginRequest("user@example.com", "password"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.Unauthorized, result.FailureType);
    }

    [Fact]
    public async Task CustomerService_GetCustomersAsync_Throws_Success()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        var result = await service.GetCustomersAsync(new PagedRequest(1, 10), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }
}
