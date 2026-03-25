using Backend.Application.Abstractions.Repositories;
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

        await repository.AddAsync(TestEntityFactory.CreateCurrency("USD", "US Dollar"));

        var duplicate = TestEntityFactory.CreateCurrency("USD", "Duplicate Dollar");

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(duplicate));
    }
}
