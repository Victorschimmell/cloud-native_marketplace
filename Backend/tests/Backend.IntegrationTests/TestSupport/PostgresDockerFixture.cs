using System.Diagnostics;
using Npgsql;

namespace Backend.IntegrationTests.TestSupport;

/// <summary>
/// xUnit collection fixture that spins up the Docker Compose postgres service before the
/// test suite runs and tears it down (container + image) once the suite finishes, whether
/// tests pass or fail.
/// </summary>
public sealed class PostgresDockerFixture : IAsyncLifetime
{
    private string _composeRoot = string.Empty;
    private string _composeProjectName = string.Empty;

    public async Task InitializeAsync()
    {
        _composeRoot = FindComposeRoot();
        _composeProjectName = $"marketplace-tests-{Guid.NewGuid():N}";
        await RunComposeAsync("up -d postgres");
        await WaitForPostgresAsync();
    }

    public async Task DisposeAsync()
    {
        await RunComposeAsync("down --rmi all -v");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static string FindComposeRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "docker-compose.yml")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "docker-compose.yml not found in any parent directory of the test output directory."
        );
    }

    private async Task RunComposeAsync(string args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -p {_composeProjectName} {args}",
                WorkingDirectory = _composeRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"'docker compose {args}' exited with code {process.ExitCode}: {error}");
        }
    }

    private static async Task WaitForPostgresAsync()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SeedTests__AdminConnectionString")
            ?? "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=postgres;Pooling=false";

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new InvalidOperationException(
            "Postgres container did not become reachable within 30 seconds.");
    }
}

[CollectionDefinition("PostgresDocker")]
public sealed class PostgresDockerCollection : ICollectionFixture<PostgresDockerFixture> { }
