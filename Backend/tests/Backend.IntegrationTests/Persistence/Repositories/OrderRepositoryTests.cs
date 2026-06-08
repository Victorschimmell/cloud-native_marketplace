using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Infrastructure.Persistence;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class OrderRepositoryTests
{
    [Fact]
    public async Task AddAsync_RejectsDuplicateOrderNumber()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = TestEntityFactory.CreateUserAccount("orders@example.com");
        var address = TestEntityFactory.CreateAddress();
        var customer = TestEntityFactory.CreateCustomer(user.Id, address.Id);

        dbContext.UserAccounts.Add(user);
        dbContext.Addresses.Add(address);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await repository.AddAsync(
            TestEntityFactory.CreateOrder(customer.Id, address.Id, "ORD-001", DateTimeOffset.UtcNow.AddMinutes(-10)),
            TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var duplicate = TestEntityFactory.CreateOrder(customer.Id, address.Id, "ORD-001", DateTimeOffset.UtcNow);
        await repository.AddAsync(duplicate, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
