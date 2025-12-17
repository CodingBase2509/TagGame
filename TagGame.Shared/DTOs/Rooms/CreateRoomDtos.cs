namespace TagGame.Shared.DTOs.Rooms;

public class CreateRoomRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public class CreateRoomResponseDto
{
    public Guid RoomId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid MembershipId { get; set; }
}
