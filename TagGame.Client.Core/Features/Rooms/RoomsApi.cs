using TagGame.Client.Core.Http;
using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Client.Core.Features.Rooms;

public sealed class RoomsApi(IApiClient apiClient) : ApiImplementationBase, IRoomsApi
{
    public async Task<CreateRoomResponseDto> CreateRoomAsync(CreateRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureValidToken(cancellationToken);
        var response = await apiClient.PostAsync<CreateRoomRequestDto, CreateRoomResponseDto>(
            "v1/rooms",
            request,
            cancellationToken);

        return response ?? throw new InvalidOperationException("Rooms API returned no content for create room.");
    }

    public async Task<JoinRoomResponseDto> JoinRoomAsync(JoinRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureValidToken(cancellationToken);
        var response = await apiClient.PostAsync<JoinRoomRequestDto, JoinRoomResponseDto>(
            "v1/rooms/join",
            request,
            cancellationToken);

        return response ?? throw new InvalidOperationException("Rooms API returned no content for join room.");
    }
}
