using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Api.Core.Persistence.Repositories;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Tests.Unit.Persistence;

public sealed class EfGenericRepositoryTests
{
    private static AuthDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    private static EfDbRepository<User> CreateUserRepo(AuthDbContext db) => new(db);

    [Fact]
    public async Task AddAsync_Persists_User_On_SaveChanges()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var u = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "A" };

        // Act
        await repo.AddAsync(u);
        await db.SaveChangesAsync();

        // Assert
        (await db.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetById_Tracked_ModifyAndSave()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Old" };
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        // Act (tracking path: options == null)
        var loaded = await repo.GetByIdAsync([user.Id], options: null);
        loaded.Should().NotBeNull();
        loaded!.DisplayName = "New";
        await db.SaveChangesAsync();

        // Assert
        (await db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id)).DisplayName.Should().Be("New");
    }

    [Fact]
    public async Task GetById_AsNoTracking_Detached_UpdateViaRepository()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Old" };
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        var options = new QueryOptions<User> { AsNoTracking = true };

        // Act
        var loaded = await repo.GetByIdAsync([user.Id], options);
        loaded.Should().NotBeNull();
        db.Entry(loaded!).State.Should().Be(EntityState.Detached);

        loaded!.DisplayName = "Newer";
        await repo.UpdateAsync(loaded);
        await db.SaveChangesAsync();

        // Assert
        (await db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id)).DisplayName.Should().Be("Newer");
    }

    [Fact]
    public async Task AddAsync_Throws_When_Duplicate_Key_Tracked()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var id = Guid.NewGuid();
        var first = new User { Id = id, CreatedAt = DateTimeOffset.UtcNow };
        await repo.AddAsync(first);
        await db.SaveChangesAsync();

        var duplicate = new User { Id = id, CreatedAt = DateTimeOffset.UtcNow };

        // Act
        var act = () => repo.AddAsync(duplicate);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task FirstOrDefault_AsTracking_Allows_Inline_Modification()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Old" };
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        // Act (options null -> defaults to tracking in our repo)
        var loaded = await repo.FirstOrDefaultAsync(u => u.Id == user.Id, options: null);
        loaded.Should().NotBeNull();
        loaded!.DisplayName = "New";
        await db.SaveChangesAsync();

        // Assert
        (await db.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id)).DisplayName.Should().Be("New");
    }

    // Note: AsNoTracking behavior is provider-dependent in tests (InMemory stores entity instances).
    // We validate update semantics via UpdateAsync in other tests.

    [Fact]
    public async Task ListAsync_OrderBy_Applies()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        await repo.AddAsync(new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Charlie" });
        await repo.AddAsync(new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Alice" });
        await repo.AddAsync(new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, DisplayName = "Bob" });
        await db.SaveChangesAsync();

        var options = new QueryOptions<User>
        {
            AsNoTracking = true,
            OrderBy = q => q.OrderBy(u => u.DisplayName)
        };

        // Act
        var list = await repo.ListAsync(options: options);

        // Assert
        list.Select(u => u.DisplayName).Should().Equal("Alice", "Bob", "Charlie");
    }

    [Fact]
    public async Task DeleteAsync_Removes_Entity()
    {
        // Arrange
        await using var db = CreateDb();
        var repo = CreateUserRepo(db);
        var user = new User { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(user);
        await db.SaveChangesAsync();

        // Assert
        (await db.Users.CountAsync()).Should().Be(0);
    }
}
