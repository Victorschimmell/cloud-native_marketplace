using Backend.Application.Abstractions.Repositories;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class UserAccountRepositoryTests
{
    [Fact]
    public async Task AddAsync_RejectsDuplicateEmail()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();

        await repository.AddAsync(TestEntityFactory.CreateUserAccount("duplicate@example.com"));

        var duplicate = TestEntityFactory.CreateUserAccount("duplicate@example.com");

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(duplicate));
    }
}
