using System.Linq.Expressions;
using Moq;
using TagGame.Api.Core.Common.Exceptions;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Features.Game;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;

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

    [Fact]
    public async Task UpdateSettingsAsync_returns_null_when_room_not_found()
    {
        var roomId = Guid.NewGuid();
        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.Setup(r => r.GetByIdAsync(
                It.Is<object[]>(k => k.Length == 1 && (Guid)k[0] == roomId),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameRoom?)null);

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var settings = await sut.UpdateSettingsAsync(roomId, hideTimeSec: 120, huntTimeSec: null, tagRadiusM: null);

        settings.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSettingsAsync_throws_when_room_not_in_lobby()
    {
        var roomId = Guid.NewGuid();
        var room = new GameRoom
        {
            Id = roomId,
            Name = "R",
            AccessCode = "ABCDEFGH",
            OwnerUserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            State = GameState.Hunt,
            Settings = new RoomSettings { HideTimeSec = 60, HuntTimeSec = 600, TagRadiusM = 4 }
        };

        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.Setup(r => r.GetByIdAsync(
                It.Is<object[]>(k => k.Length == 1 && (Guid)k[0] == roomId),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var act = async () => await sut.UpdateSettingsAsync(roomId, hideTimeSec: 120, huntTimeSec: null, tagRadiusM: null);

        await act.Should().ThrowAsync<DomainRuleViolationException>()
            .Where(e => e.Message == "Errors.Rooms.Settings.EditOnlyInLobby");
    }

    [Fact]
    public async Task UpdateSettingsAsync_updates_only_provided_values()
    {
        var roomId = Guid.NewGuid();
        var room = new GameRoom
        {
            Id = roomId,
            Name = "R",
            AccessCode = "ABCDEFGH",
            OwnerUserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            State = GameState.Lobby,
            Settings = new RoomSettings { HideTimeSec = 60, HuntTimeSec = 600, TagRadiusM = 4 }
        };

        var repo = new Mock<IDbRepository<GameRoom>>();
        repo.Setup(r => r.GetByIdAsync(
                It.Is<object[]>(k => k.Length == 1 && (Guid)k[0] == roomId),
                It.IsAny<QueryOptions<GameRoom>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var uow = new Mock<IGamesUoW>();
        uow.SetupGet(x => x.Rooms).Returns(repo.Object);

        var sut = new RoomService(uow.Object, TimeProvider.System);

        var settings = await sut.UpdateSettingsAsync(roomId, hideTimeSec: 120, huntTimeSec: null, tagRadiusM: 5.5);

        settings.Should().NotBeNull();
        settings!.HideTimeSec.Should().Be(120);
        settings.HuntTimeSec.Should().Be(600);
        settings.TagRadiusM.Should().Be(5.5);
    }
}
