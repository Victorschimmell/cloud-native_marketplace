using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceSkeletonTests
{
    [Fact]
    public async Task ProductService_GetByIdAsync_Throws_NotImplementedException()
    {
        var service = new ProductService(new FakeProductRepository(), new FakeProductListingRepository(), new FakeCurrencyConversionService());

        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.NotImplemented, result.FailureType);
    }

    [Fact]
    public async Task AuthService_LoginAsync_Throws_NotImplementedException()
    {
        var service = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeUnitOfWork());

        var result = await service.LoginAsync(new LoginRequest("user@example.com", "password"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.Unauthorized, result.FailureType);
    }

    [Fact]
    public async Task CustomerService_GetCustomersAsync_Throws_NotImplementedException()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        var result = await service.GetCustomersAsync(new PagedRequest(1, 10), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultFailureType.NotImplemented, result.FailureType);
    }
}
