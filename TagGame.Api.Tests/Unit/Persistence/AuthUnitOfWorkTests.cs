using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Api.Core.Persistence.Repositories;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Tests.Unit.Persistence;

public sealed class AuthUnitOfWorkTests
{
    private static AuthDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_Commits_Pending_Changes()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = new EfDbRepository<User>(db);
        var uow = new AuthUnitOfWork(db, new ServiceCollection().BuildServiceProvider());

        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
        await repo.AddAsync(user);

        // Act
        var written = await uow.SaveChangesAsync();

        // Assert
        written.Should().BeGreaterThan(0);
        (await db.Users.CountAsync()).Should().Be(1);
    }
}
