using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await repository.AddAsync(TestEntityFactory.CreateUserAccount("duplicate@example.com"), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var duplicate = TestEntityFactory.CreateUserAccount("duplicate@example.com");
        await repository.AddAsync(duplicate, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
