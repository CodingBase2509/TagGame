namespace TagGame.Shared.DTOs.Settings;

public class PatchRoomSettingsRequestDto
{
    public int? HideTimeSec { get; set; }

    public int? HuntTimeSec { get; set; }

    public double? TagRadiusM { get; set; }
}

public class RoomSettingsResponseDto
{
    public int HideTimeSec { get; set; }

    public int HuntTimeSec { get; set; }

    public double TagRadiusM { get; set; }
}
