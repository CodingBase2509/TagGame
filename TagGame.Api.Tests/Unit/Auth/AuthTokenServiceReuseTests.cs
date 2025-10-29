using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Persistence.Repositories;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Tests.Unit.Auth;

public sealed class AuthTokenServiceReuseTests
{
    private static JwtOptions CreateOptions() => new()
    {
        Issuer = "TagGame",
        Audience = "TagGameClient",
        SigningKey = "unit-test-signing-key-1234567890",
        AccessMinutes = 5,
        RefreshDays = 7
    };

    private static (AuthDbContext db, IAuthTokenService svc, IAuthUoW uow, DateTimeOffset now) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AuthDbContext(options);
        var now = DateTimeOffset.UtcNow;
        var tp = new FixedTimeProvider(now);

        var logger = new Mock<ILogger<AuthTokenService>>().Object;
        var sp = new ServiceCollection()
            .AddSingleton(db)
            .AddScoped(typeof(IDbRepository<>), typeof(EfDbRepository<>))
            .BuildServiceProvider();
        IAuthUoW uow = new AuthUnitOfWork(db, sp);
        var svc = new AuthTokenService(uow, Options.Create(CreateOptions()), tp, logger);

        return (db, svc, uow, now);
    }

    [Fact]
    public async Task RefreshToken_Reuse_Detected_Revokes_Family_And_Throws()
    {
        // Arrange
        var (db, svc, uow, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = now };
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();

        var pair1 = await svc.IssueTokenAsync(user);
        var pair2 = await svc.RefreshTokenAsync(pair1.RefreshToken);

        // Act
        var act = async () => await svc.RefreshTokenAsync(pair1.RefreshToken);

        // Assert
        var thrown = await act.Should().ThrowAsync<Exception>();
        thrown.Which.GetType().Name.Should().Contain("RefreshTokenReuse");

        var family = db.RefreshTokens.Select(x => x.FamilyId).Distinct().Single();
        var all = db.RefreshTokens.Where(x => x.FamilyId == family).ToList();
        all.Should().NotBeEmpty();
        all.Should().OnlyContain(t => t.RevokedAt.HasValue);
    }

    [Fact]
    public async Task After_Reuse_Family_Revoked_Subsequent_Refresh_Fails()
    {
        // Arrange
        var (db, svc, uow, now) = CreateSut();
        var user = new User { Id = Guid.NewGuid(), CreatedAt = now };
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();

        var pair1 = await svc.IssueTokenAsync(user);
        var pair2 = await svc.RefreshTokenAsync(pair1.RefreshToken);

        // Trigger reuse (revokes family)
        Func<Task> reuseAct = () => svc.RefreshTokenAsync(pair1.RefreshToken);
        var reuse = await reuseAct.Should().ThrowAsync<Exception>();
        reuse.Which.GetType().Name.Should().Contain("RefreshTokenReuse");

        // Act
        var act = async () => await svc.RefreshTokenAsync(pair2.RefreshToken);

        // Assert: depending on implementation, may throw revoked or reuse; accept both
        var next = await act.Should().ThrowAsync<Exception>();
        next.Which.Message.Should().NotBeNullOrWhiteSpace();
        next.Which.Message.Should().ContainEquivalentOf("revoked");
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
