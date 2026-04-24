using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.IntegrationTests;

public sealed class CurrencyRepositoryTests
{
    [Fact]
    public async Task AddAsync_RejectsDuplicateCode()
    {
        await using var host = await SqliteRepositoryTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await repository.AddAsync(TestEntityFactory.CreateCurrency("USD", "US Dollar"), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var duplicate = TestEntityFactory.CreateCurrency("USD", "Duplicate Dollar");
        await repository.AddAsync(duplicate, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
