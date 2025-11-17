using Npgsql;
using Testcontainers.PostgreSql;

namespace TagGame.Api.Tests.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    private protected PostgreSqlContainer? _dbContainer;

    protected void UseDbTestContainer()
    {
        if (!DockerRequirement.IsAvailable)
        {
            _dbContainer = null;
            return;
        }

        var dbGuid = Guid.NewGuid();
        _dbContainer = new PostgreSqlBuilder()
            .WithName("TestDb" + dbGuid)
            .WithDatabase("TestDb" + dbGuid)
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        if (_dbContainer is null)
            return;

        await _dbContainer.StartAsync();
        var baseCs = _dbContainer.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", baseCs);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", baseCs);
    }

    public virtual async Task DisposeAsync()
    {
        if (_dbContainer is null)
            return;
        await _dbContainer.DisposeAsync();
    }

    protected async Task<string> CreateDatabaseAsync(string? nameHint = null)
    {
        if (_dbContainer is null)
            throw new InvalidOperationException("DB container not initialized. Call UseDbTestContainer() before CreateDatabaseAsync().");

        var dbName = $"it_{nameHint ?? "db"}_{Guid.NewGuid():N}";
        await using var conn = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\";", conn);
        await cmd.ExecuteNonQueryAsync();

        var csb = new NpgsqlConnectionStringBuilder(_dbContainer.GetConnectionString()) { Database = dbName };
        return csb.ToString();
    }
}
