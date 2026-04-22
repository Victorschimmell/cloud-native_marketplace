using Backend.Api;
using Backend.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.IntegrationTests;

public class MarketplaceApiWithSeedingFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgresSeederTestHost? _testHost;

    public async Task InitializeAsync()
    {
        _testHost = await PostgresSeederTestHost.CreateAsync(_ => { });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        if (_testHost != null)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _testHost.ConnectionString);
        }

        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
    }

    public async Task DisposeAsync()
    {
        if (_testHost != null)
        {
            await _testHost.DisposeAsync();
        }
    }
}