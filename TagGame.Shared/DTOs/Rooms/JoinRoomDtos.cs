namespace TagGame.Shared.DTOs.Rooms;

public class JoinRoomRequestDto
{
    public string AccessCode { get; set; } = string.Empty;
}

public class JoinRoomResponseDto
{
    public Guid RoomId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid MembershipId { get; set; }
}
