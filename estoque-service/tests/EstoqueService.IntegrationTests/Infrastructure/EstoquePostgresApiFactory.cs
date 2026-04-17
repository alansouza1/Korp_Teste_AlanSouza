using EstoqueService.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace EstoqueService.IntegrationTests.Infrastructure;

public sealed class EstoquePostgresApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string _databaseName = $"estoque_concurrency_tests_{Guid.NewGuid():N}";
    private readonly string _serverConnectionString;
    private readonly string _databaseConnectionString;

    public static bool IsEnabled =>
        string.Equals(
            Environment.GetEnvironmentVariable("ENABLE_POSTGRES_CONCURRENCY_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    public EstoquePostgresApiFactory()
    {
        var baseConnectionString =
            Environment.GetEnvironmentVariable("ESTOQUE_POSTGRES_TEST_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        var serverBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres"
        };

        _serverConnectionString = serverBuilder.ConnectionString;

        var databaseBuilder = new NpgsqlConnectionStringBuilder(_serverConnectionString)
        {
            Database = _databaseName
        };

        _databaseConnectionString = databaseBuilder.ConnectionString;

        CreateDatabase();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<EstoqueDbContext>));
            services.RemoveAll(typeof(EstoqueDbContext));

            services.AddDbContext<EstoqueDbContext>(options =>
                options.UseNpgsql(
                    _databaseConnectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(EstoqueDbContext).Assembly.FullName)));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await DropDatabaseAsync();
        GC.SuppressFinalize(this);
    }

    private void CreateDatabase()
    {
        using var connection = new NpgsqlConnection(_serverConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{_databaseName}";""";
        command.ExecuteNonQuery();
    }

    private async Task DropDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_serverConnectionString);
        await connection.OpenAsync();

        await using (var terminateCommand = connection.CreateCommand())
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

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"""DROP DATABASE IF EXISTS "{_databaseName}";""";
        await dropCommand.ExecuteNonQueryAsync();
    }
}
