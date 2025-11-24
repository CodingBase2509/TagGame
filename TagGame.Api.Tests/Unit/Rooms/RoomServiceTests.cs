using System.Linq.Expressions;
using Moq;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Features.Game;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Tests.Unit.Rooms;

public class RoomServiceTests
{
    [Fact]
    public async Task GenerateUniqueAccessCodeAsync_returns_alpha_numeric_8_chars()
    {
        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<GameRoom, bool>>>(),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameRoom?)null);

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var code = await sut.GenerateUniqueAccessCodeAsync();

        code.Should().MatchRegex("^[A-Za-z0-9]{8}$");
    }

    [Fact]
    public async Task GenerateUniqueAccessCodeAsync_retries_on_collision()
    {
        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<GameRoom, bool>>>(),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameRoom { AccessCode = "dup" })
            .ReturnsAsync((GameRoom?)null);

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var code = await sut.GenerateUniqueAccessCodeAsync();

        code.Should().MatchRegex("^[A-Za-z0-9]{8}$");
        repo.Verify(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<GameRoom, bool>>>(),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GenerateUniqueAccessCodeAsync_throws_after_exhausting_attempts()
    {
        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<GameRoom, bool>>>(),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameRoom());

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var act = async () => await sut.GenerateUniqueAccessCodeAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to generate unique access code*");

        repo.Verify(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<GameRoom, bool>>>(),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(20));
    }
}
