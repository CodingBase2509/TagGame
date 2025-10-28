using System.Diagnostics;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using Xunit.Abstractions;

namespace TagGame.Api.Tests.Integration.Auth;

public sealed class AuthCrudTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private TestcontainersContainer? _pg;
    private DockerPg? _pgFallback;
    private readonly string _dbName = $"taggame_authtest_{Guid.NewGuid():N}";
    private IFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        // Arrange: Testcontainers Postgres (ephemeral)
        // Disable Ryuk (resource reaper) to avoid hijack/attach issues in some local Docker setups.
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        string csAdmin;
        try
        {
            _pg = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("postgres:16-alpine")
                .WithEnvironment("POSTGRES_USER", "taggame")
                .WithEnvironment("POSTGRES_PASSWORD", "SecurePassword")
                .WithEnvironment("POSTGRES_DB", "postgres")
                .WithPortBinding(0, 5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
            await _pg.StartAsync();

            var mappedPort = _pg.GetMappedPublicPort(5432);
            csAdmin = $"Host=localhost;Port={mappedPort};Database=postgres;Username=taggame;Password=SecurePassword;Include Error Detail=true";
        }
        catch
        {
            // Fallback for environments where Testcontainers cannot hijack/attach (e.g., Docker contexts)
            _pgFallback = await DockerPg.StartAsync();
            csAdmin = _pgFallback.AdminConnectionString;
        }

        // Setup: AutoFixture with AutoMoq
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        // Create a dedicated database for tests
        await using (var conn = new Npgsql.NpgsqlConnection(csAdmin))
        {
            await conn.OpenAsync();
            await using var cmd = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{_dbName}\";", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        // Point EF at the test database for design-time factory
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(csAdmin)
        {
            Database = _dbName
        };
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", builder.ToString());

        // Apply migrations
        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_pg is not null)
            await _pg.DisposeAsync();
        if (_pgFallback is not null)
            await _pgFallback.DisposeAsync();
    }

    [Fact]
    public async Task Migrations_Apply_And_BasicCrud_Works()
    {
        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);

        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.Id, Guid.NewGuid())
            .With(u => u.DisplayName, "Test User")
            .With(u => u.Email, (string?)null)
            .With(u => u.DeviceId, "device-123")
            .With(u => u.AvatarColor, "#FFAA00")
            .With(u => u.Flags, 0)
            .With(u => u.CreatedAt, DateTimeOffset.UtcNow)
            .With(u => u.LastSeenAt, (DateTimeOffset?)null)
            .Create();

        ctx.Users.Add(user);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save user failed: {ex}");
            throw;
        }

        var token1 = _fixture.Build<RefreshToken>()
            .With(t => t.Id, Guid.NewGuid())
            .With(t => t.UserId, user.Id)
            .With(t => t.FamilyId, Guid.NewGuid())
            .With(t => t.TokenHash, Guid.NewGuid().ToString("N"))
            .With(t => t.CreatedAt, DateTimeOffset.UtcNow)
            .With(t => t.ExpiresAt, DateTimeOffset.UtcNow.AddDays(7))
            .With(t => t.RevokedAt, (DateTimeOffset?)null)
            .With(t => t.ReplacedById, (Guid?)null)
            .Create();

        ctx.RefreshTokens.Add(token1);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token1 failed: {ex}");
            throw;
        }

        var token2 = _fixture.Build<RefreshToken>()
            .With(t => t.Id, Guid.NewGuid())
            .With(t => t.UserId, user.Id)
            .With(t => t.FamilyId, token1.FamilyId)
            .With(t => t.TokenHash, Guid.NewGuid().ToString("N"))
            .With(t => t.CreatedAt, DateTimeOffset.UtcNow)
            .With(t => t.ExpiresAt, DateTimeOffset.UtcNow.AddDays(7))
            .With(t => t.RevokedAt, (DateTimeOffset?)null)
            .With(t => t.ReplacedById, (Guid?)null)
            .Create();

        // Act
        ctx.RefreshTokens.Add(token2);
        token1.RevokedAt = DateTimeOffset.UtcNow;
        token1.ReplacedById = token2.Id;
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token2 failed: {ex}");
            throw;
        }

        var tokensForUser = ctx.RefreshTokens.Where(t => t.UserId == user.Id).ToList();

        // Assert
        tokensForUser.Count.Should().Be(2);
        tokensForUser.Should().ContainSingle(t => t.Id == token1.Id)
            .Which.Should().Match<RefreshToken>(t => t.RevokedAt.HasValue && t.ReplacedById == token2.Id);
        tokensForUser.Should().ContainSingle(t => t.Id == token2.Id)
            .Which.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Deleting_User_Cascades_To_RefreshTokens()
    {
        await using var ctx = new DesignTimeAuthFactory().CreateDbContext([]);

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            LastSeenAt = null,
            DisplayName = "User",
            Email = null,
            DeviceId = "dev-1",
            AvatarColor = "#000000",
            Flags = 0
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = Guid.NewGuid(),
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        ctx.RefreshTokens.Add(token);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine($"Save token failed: {ex}");
            throw;
        }

        // Act
        ctx.Users.Remove(user);
        await ctx.SaveChangesAsync();

        // Assert
        ctx.RefreshTokens.Count(t => t.UserId == user.Id).Should().Be(0);
    }
}

// Simple docker wrapper fallback for environments where Testcontainers cannot attach/hijack streams.
internal sealed class DockerPg(string containerId, int hostPort) : IAsyncDisposable
{
    public string AdminConnectionString => $"Host=localhost;Port={hostPort};Database=postgres;Username=taggame;Password=SecurePassword;Include Error Detail=true";

    public async ValueTask DisposeAsync()
    {
        try { await RunProcessAsync("docker", $"rm -f {containerId}"); }
        catch { /* ignore */ }
    }

    public static async Task<DockerPg> StartAsync()
    {
        var runArgs = "run --rm -d -e POSTGRES_USER=taggame -e POSTGRES_PASSWORD=SecurePassword -e POSTGRES_DB=postgres -p 0:5432 postgres:16-alpine";
        var id = (await RunProcessAsync("docker", runArgs)).Trim();
        var portOut = await RunProcessAsync("docker", $"port {id} 5432/tcp");
        var first = portOut.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
        var port = int.Parse(first.Split(':').Last());

        var cs = $"Host=localhost;Port={port};Database=postgres;Username=taggame;Password=SecurePassword;Include Error Detail=true";
        var ready = await WaitForAsync(async () =>
        {
            try
            {
                await using var c = new Npgsql.NpgsqlConnection(cs);
                await c.OpenAsync();
                return true;
            }
            catch { return false; }
        }, TimeSpan.FromSeconds(30));
        if (!ready)
        {
            var logs = await RunProcessAsync("docker", $"logs --tail=80 {id}");
            throw new InvalidOperationException($"Postgres not ready. Logs:\n{logs}");
        }
        return new DockerPg(id, port);
    }

    private static async Task<string> RunProcessAsync(string file, string args)
    {
        var psi = new ProcessStartInfo { FileName = file, Arguments = args, RedirectStandardOutput = true, RedirectStandardError = true };
        using var p = Process.Start(psi)!;
        var o = await p.StandardOutput.ReadToEndAsync();
        var e = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        return p.ExitCode != 0 ? throw new InvalidOperationException($"{file} {args} failed: {e}") : o;
    }

    private static async Task<bool> WaitForAsync(Func<Task<bool>> pred, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (await pred()) return true;
            await Task.Delay(500);
        }
        return false;
    }
}
