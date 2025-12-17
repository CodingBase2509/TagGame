using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Client.Core.Features.Rooms;

public interface IRoomsApi
{
    Task<CreateRoomResponseDto> CreateRoomAsync(CreateRoomRequestDto request, CancellationToken cancellationToken = default);

    Task<JoinRoomResponseDto> JoinRoomAsync(JoinRoomRequestDto request, CancellationToken cancellationToken = default);
}
