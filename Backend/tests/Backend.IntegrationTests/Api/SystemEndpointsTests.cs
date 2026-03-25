using Backend.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

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
    public async Task WeatherForecastEndpointReturnsExpectedForecasts()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Count);
        Assert.All(forecasts, forecast =>
        {
            Assert.InRange(forecast.TemperatureC, -20, 54);
            Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF);
            Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
            Assert.True(forecast.Date > DateOnly.FromDateTime(DateTime.Today));
        });
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", null);
    }
}
