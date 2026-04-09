using Backend.Application.Common.Models;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ServiceGuardBehaviorTests
{
    [Fact]
    public async Task ProductService_GetByIdAsync_Fails_For_Empty_Id()
    {
        var service = new ProductService(new FakeProductRepository(), new FakeUnitOfWork());

        var result = await service.GetByIdAsync(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("Product id is required.", result.Error);
    }

    [Fact]
    public async Task CartService_AddItemAsync_Fails_For_Invalid_Quantity()
    {
        var service = new CartService(new FakeCartRepository(), new FakeProductListingRepository(), new FakeDateTimeProvider(), new FakeUnitOfWork());

        var result = await service.AddItemAsync(new AddCartItemRequest(null, Guid.NewGuid(), null, Guid.NewGuid(), 0));

        Assert.True(result.IsFailure);
        Assert.Equal("Listing id is required and quantity must be greater than zero.", result.Error);
    }

    [Fact]
    public async Task AuthService_LoginAsync_Fails_For_Missing_Credentials()
    {
        var service = new AuthService(new FakeUserAccountRepository(), new FakePasswordHasher(), new FakeAuthTokenGenerator(), new FakeDateTimeProvider(), new FakeUnitOfWork());

        var result = await service.LoginAsync(new LoginRequest("", ""));

        Assert.True(result.IsFailure);
        Assert.Equal("Email and password are required.", result.Error);
    }

    [Fact]
    public async Task CustomerService_GetCustomersAsync_Fails_For_Invalid_Paging()
    {
        var service = new CustomerService(new FakeCustomerRepository());

        var result = await service.GetCustomersAsync(new PagedRequest(0, 10));

        Assert.True(result.IsFailure);
        Assert.Equal("Page and page size must be greater than zero.", result.Error);
    }
}
