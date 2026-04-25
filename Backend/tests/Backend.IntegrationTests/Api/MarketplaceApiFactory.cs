using Backend.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace Backend.IntegrationTests;

public class MarketplaceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _databaseName = default!;
    private string _connectionString = default!;
    private readonly string _adminConnectionString;

    public MarketplaceApiFactory()
    {
        _adminConnectionString = Environment.GetEnvironmentVariable("SeedTests__AdminConnectionString") ??
            "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=postgres;Pooling=false";
    }

    public async ValueTask InitializeAsync()
    {
        _databaseName = $"marketplace_api_test_{Guid.NewGuid():N}";

        await using var adminConnection = new NpgsqlConnection(_adminConnectionString);
        await adminConnection.OpenAsync();

        await using var createCommand = adminConnection.CreateCommand();
        createCommand.CommandText = $"""CREATE DATABASE "{_databaseName}" """;
        await createCommand.ExecuteNonQueryAsync();

        var builder = new NpgsqlConnectionStringBuilder(_adminConnectionString)
        {
            Database = _databaseName,
            Pooling = false
        };
        _connectionString = builder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    }

    public override async ValueTask DisposeAsync()
    {
        await using var adminConnection = new NpgsqlConnection(_adminConnectionString);
        await adminConnection.OpenAsync();

        await using (var terminateCommand = adminConnection.CreateCommand())
        {
            terminateCommand.CommandText =
                """
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @databaseName
                  AND pid <> pg_backend_pid();
                """;
            terminateCommand.Parameters.AddWithValue("databaseName", _databaseName);
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using var dropCommand = adminConnection.CreateCommand();
        dropCommand.CommandText = $"""DROP DATABASE IF EXISTS "{_databaseName}" """;
        await dropCommand.ExecuteNonQueryAsync();

        await base.DisposeAsync();
    }
}
