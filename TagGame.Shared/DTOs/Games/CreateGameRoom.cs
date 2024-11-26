namespace TagGame.Shared.DTOs.Games;

public static class CreateGameRoom
{
    public class CreateGameRoomRequest
    {
        public Guid UserId { get; set; }

        public string GameRoomName { get; set; } = string.Empty;
    }

    public class CreateGameRoomResponse
    {
        public Guid RoomId { get; set; }

        public string AccessCode { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;
    }
}
