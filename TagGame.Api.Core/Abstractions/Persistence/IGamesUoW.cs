using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Abstractions.Persistence;

public interface IGamesUoW : IUnitOfWork
{
    IDbRepository<GameRoom> Rooms { get; }

    IDbRepository<RoomMembership> RoomMemberships { get; }

    IDbRepository<Match> Matches { get; }

    IDbRepository<Round> Rounds { get; }
}
