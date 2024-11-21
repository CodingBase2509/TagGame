using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.DTOs.Games;

public class JoinGameRoom
{
    public class Request
    {
        public Guid UserId { get; set; }

        public string GameName { get; set;} = string.Empty;

        public string AccessCode { get; set; } = string.Empty;
    }

    public class Response
    {
        public GameRoom Room { get; set; }

        public Player Player { get; set; }
    }
}
