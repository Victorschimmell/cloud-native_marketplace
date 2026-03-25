using Backend.Application.Abstractions.Repositories;
using Backend.IntegrationTests.TestSupport;
using Backend.Infrastructure.Persistence;
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

        var user = TestEntityFactory.CreateUserAccount("orders@example.com");
        var address = TestEntityFactory.CreateAddress();
        var customer = TestEntityFactory.CreateCustomer(user.Id, address.Id);

        dbContext.UserAccounts.Add(user);
        dbContext.Addresses.Add(address);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        await repository.AddAsync(TestEntityFactory.CreateOrder(customer.Id, address.Id, "ORD-001", DateTimeOffset.UtcNow.AddMinutes(-10)));

        var duplicate = TestEntityFactory.CreateOrder(customer.Id, address.Id, "ORD-001", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(duplicate));
    }
}
