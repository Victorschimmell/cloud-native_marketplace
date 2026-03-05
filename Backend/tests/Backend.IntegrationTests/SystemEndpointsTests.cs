using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.IntegrationTests;

public class SystemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestConnectionString = "Host=localhost;Port=5433;Database=marketplace;Username=postgres;Password=postgres";

    public SystemEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestConnectionString);
        Environment.SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", "false");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public async Task HealthEndpointReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task SystemInfoEndpointReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/system/info");

        Assert.True(response.IsSuccessStatusCode);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", null);
    }
}
