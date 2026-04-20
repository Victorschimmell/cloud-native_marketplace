using Backend.Application.Common.Models;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceSkeletonTests
{
    [Fact]
    public async Task ProductService_GetByIdAsync_Throws_NotImplementedException()
    {
        var service = new ProductService(new FakeProductRepository());

        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AuthService_LoginAsync_Throws_NotImplementedException()
    {
        var service = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider());

        await Assert.ThrowsAsync<NotImplementedException>(() => service.LoginAsync(new LoginRequest("user@example.com", "password")));
    }

    [Fact]
    public async Task CustomerService_GetCustomersAsync_Throws_NotImplementedException()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetCustomersAsync(new PagedRequest(1, 10)));
    }
}
