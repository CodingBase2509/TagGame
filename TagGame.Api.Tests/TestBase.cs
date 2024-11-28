global using AutoFixture;
global using FluentAssertions;
global using Moq;
global using Xunit;
using Testcontainers.PostgreSql;

namespace TagGame.Api.Tests;

public class TestBase : IAsyncLifetime
{
    private protected Fixture _fixture;
    private protected PostgreSqlContainer? _dbContainer;
    
    public TestBase()
    {
        _fixture = new Fixture();
        var customization = new SupportMutableValueTypesCustomization();
        _fixture.Customize(customization);
    }

    void UseDbTestContainer()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("TestDb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (_dbContainer is null)
            return;
        
        await _dbContainer.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        if (_dbContainer is null)
            return;
        await _dbContainer.DisposeAsync();
    }
}