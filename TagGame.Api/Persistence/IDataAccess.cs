using TagGame.Api.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Persistence;

public interface IDataAccess
{
    IDataSet<GameRoom> Rooms { get; }
    IDataSet<GameSettings> Settings { get; }
    IDataSet<Player> Players { get; }
    IDataSet<User> Users { get; }
    IDataSet<PlayerLeftGameInfo> PlayerLeftInfo { get; }

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}