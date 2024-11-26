using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Client.Services;

public class RestClient(ConfigHandler configHandler)
{
    private HttpClient client;
    
    public async Task<Response<CreateGameRoom.CreateGameRoomResponse>?> CreateRoomAsync(CreateGameRoom.CreateGameRoomRequest createGameRoomRequest)
    {
        await InitAsync();
        
        var stringContent = JsonSerializer.Serialize(createGameRoomRequest, MappingOptions.JsonSerializerOptions);
        var response = client.PostAsync(ApiRoutes.GameRoom.CreateRoom,
            new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
        
        var content = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Response<CreateGameRoom.CreateGameRoomResponse>>(content, MappingOptions.JsonSerializerOptions);
    }

    public async Task<Response<JoinGameRoom.JoinGameRoomResponse>?> JoinRoomAsync(JoinGameRoom.JoinGameRoomRequest joinGameRoomRequest)
    {
        await InitAsync();
        
        var stringContent = JsonSerializer.Serialize(joinGameRoomRequest, MappingOptions.JsonSerializerOptions);
        var response = client.PostAsync(ApiRoutes.GameRoom.JoinRoom,
            new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
        
        var content = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Response<JoinGameRoom.JoinGameRoomResponse>>(content, MappingOptions.JsonSerializerOptions);
    }

    public async Task<Response<GameRoom>?> GetRoomAsync(Guid roomId)
    {
        await InitAsync();
        string route = ApiRoutes.GameRoom.GroupName 
                       + ApiRoutes.GameRoom.GetRoom.Replace("{roomId:guid}", roomId.ToString());
        var response = client.GetAsync(route);
        
        var content = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Response<GameRoom>>(content, MappingOptions.JsonSerializerOptions);
    }

    public async Task<bool> UpdateSettingsAsync(GameSettings settings)
    {
        await InitAsync();
        var stringContent = JsonSerializer.Serialize(settings, MappingOptions.JsonSerializerOptions);
        var response = client.PutAsync(ApiRoutes.GameRoom.UpdateSettings,
            new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
        
        var content = await response.Result.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Response>(content, MappingOptions.JsonSerializerOptions);
        return result is not null && result.IsSuccess;
    }

    public async Task<Response<User>?> CreateUserAsync(CreateUser.CreateUserRequest createUserRequest)
    {
        await InitAsync();
        var stringContent = JsonSerializer.Serialize(createUserRequest, MappingOptions.JsonSerializerOptions);
        var response = client.PutAsync(ApiRoutes.GameRoom.UpdateSettings,
            new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
        
        var content = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Response<User>>(content, MappingOptions.JsonSerializerOptions);
    }

    private async Task InitAsync()
    {
        if (client is not null)
            return;
            
        var serverConfig = await configHandler.ReadAsync<ServerConfig>();
        var userConfig = await configHandler.ReadAsync<UserConfig>();
        
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userConfig.UserId.ToString());
        var userId = Convert.ToBase64String(plainTextBytes);
        
        var baseAddressString = serverConfig?.Port is null ? serverConfig?.Host : $"{serverConfig.Host}:{serverConfig.Port}";
        
        client = new HttpClient()
        {
            BaseAddress = new Uri(baseAddressString),
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json) },
                Authorization = new AuthenticationHeaderValue("Bearer", userId)
            }
        };
    }
}