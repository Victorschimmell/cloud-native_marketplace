using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Backend.Infrastructure.Persistence.Import.Services;
using Backend.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Backend.IntegrationTests.TestSupport;

internal sealed class PostgresSeederTestHost : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _databaseName;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _testConnectionString;

    private PostgresSeederTestHost(string adminConnectionString, string databaseName, ServiceProvider serviceProvider, string testConnectionString)
    {
        _adminConnectionString = adminConnectionString;
        _databaseName = databaseName;
        _serviceProvider = serviceProvider;
        _testConnectionString = testConnectionString;
    }

    public string ConnectionString => _testConnectionString;

    public static async Task<PostgresSeederTestHost> CreateAsync(Action<OlistSeedOptions> configureOlistOptions)
    {
        var adminConnectionString =
            Environment.GetEnvironmentVariable("SeedTests__AdminConnectionString") ??
            "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=postgres;Pooling=false";

        var databaseName = $"marketplace_seed_{Guid.NewGuid():N}";

        try
        {
            await using var adminConnection = new NpgsqlConnection(adminConnectionString);
            await adminConnection.OpenAsync();

            await using var createCommand = adminConnection.CreateCommand();
            createCommand.CommandText = $"""CREATE DATABASE "{databaseName}" """;
            await createCommand.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "PostgreSQL seeding tests require a reachable admin database. Start Docker Compose postgres before running this suite.",
                exception);
        }

        var databaseConnectionStringBuilder = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            Database = databaseName,
            Pooling = false
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(databaseConnectionStringBuilder.ConnectionString));
        services.AddScoped<Backend.Application.Common.Abstractions.IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICsvDatasetReader, CsvDatasetReader>();
        services.AddScoped<IOlistDataSeeder, OlistDataSeeder>();

        var olistOptions = new OlistSeedOptions();
        configureOlistOptions(olistOptions);
        services.AddSingleton(Options.Create(olistOptions));

        var serviceProvider = services.BuildServiceProvider();

        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        return new PostgresSeederTestHost(adminConnectionString, databaseName, serviceProvider, databaseConnectionStringBuilder.ConnectionString);
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();

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
    }
}
