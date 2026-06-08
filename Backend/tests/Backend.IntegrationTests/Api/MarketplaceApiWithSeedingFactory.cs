using Backend.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.IntegrationTests;

public class MarketplaceApiWithSeedingFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgresSeederTestHost? _testHost;

    public async ValueTask InitializeAsync()
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

    public override async ValueTask DisposeAsync()
    {
        if (_testHost != null)
        {
            await _testHost.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
