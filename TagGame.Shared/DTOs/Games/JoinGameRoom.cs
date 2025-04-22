using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.DTOs.Games;

public static class JoinGameRoom
{
    public class JoinGameRoomRequest
    {
        public Guid UserId { get; set; }

        public string GameName { get; set;} = string.Empty;

        public string AccessCode { get; set; } = string.Empty;
    }

    public class JoinGameRoomResponse
    {
        public GameRoom Room { get; set; }

        public Guid PlayerId { get; set; }
    }
}
