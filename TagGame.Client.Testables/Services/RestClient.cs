using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Users;

[assembly: InternalsVisibleTo("TagGame.Client.Tests")]
namespace TagGame.Client.Services;

public class RestClient(ConfigHandler configHandler)
{
    private HttpClient _client;

    internal RestClient(ConfigHandler configHandler, HttpClient client)
        : this(configHandler)
    {
        _client = client;
    }
    
    public async Task<Response<CreateGameRoom.CreateGameRoomResponse>?> CreateRoomAsync(CreateGameRoom.CreateGameRoomRequest createGameRoomRequest)
    {
        await InitAsync();

        try
        {
            var stringContent = JsonSerializer.Serialize(createGameRoomRequest, MappingOptions.JsonSerializerOptions);
            var response = await _client.PostAsync(ApiRoutes.GameRoom.CreateRoom,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<CreateGameRoom.CreateGameRoomResponse>>(content, MappingOptions.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return (Response<CreateGameRoom.CreateGameRoomResponse>) new Error(400, $"Error on sending request");
        }
    }

    public async Task<Response<JoinGameRoom.JoinGameRoomResponse>?> JoinRoomAsync(JoinGameRoom.JoinGameRoomRequest joinGameRoomRequest)
    {
        await InitAsync();

        try
        {
            var stringContent = JsonSerializer.Serialize(joinGameRoomRequest, MappingOptions.JsonSerializerOptions);
            var response = await _client.PostAsync(ApiRoutes.GameRoom.JoinRoom,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<JoinGameRoom.JoinGameRoomResponse>>(content, MappingOptions.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return (Response<JoinGameRoom.JoinGameRoomResponse>) new Error(400, "Error on sending request");
        }
    }

    public async Task<Response<GameRoom>?> GetRoomAsync(Guid roomId)
    {
        await InitAsync();

        try
        {
            string route = ApiRoutes.GameRoom.GroupName 
                           + ApiRoutes.GameRoom.GetRoom.Replace("{roomId:guid}", roomId.ToString());
            var response = await _client.GetAsync(route);
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<GameRoom>>(content, MappingOptions.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return (Response<GameRoom>) new Error(400, "Error on sending request");
        }
    }

    public async Task<bool> UpdateSettingsAsync(GameSettings settings)
    {
        await InitAsync();
        
        try
        {
            var stringContent = JsonSerializer.Serialize(settings, MappingOptions.JsonSerializerOptions);
            var response = await _client.PutAsync(ApiRoutes.GameRoom.UpdateSettings,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Response<string>>(content, MappingOptions.JsonSerializerOptions);
            return result is not null && result.IsSuccess;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<Response<User>?> CreateUserAsync(CreateUser.CreateUserRequest createUserRequest)
    {
        await InitAsync();

        try
        {
            var stringContent = JsonSerializer.Serialize(createUserRequest, MappingOptions.JsonSerializerOptions);
            var response = await _client.PutAsync(ApiRoutes.GameRoom.UpdateSettings,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<User>>(content, MappingOptions.JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return (Response<User>) new Error(400, "Error on sending request");
        }
    }

    private async Task InitAsync()
    {
        if (_client is not null)
            return;
            
        var serverConfig = await configHandler.ReadAsync<ServerConfig>();
        var userConfig = await configHandler.ReadAsync<UserConfig>();
        
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userConfig.UserId.ToString());
        var userId = Convert.ToBase64String(plainTextBytes);
        
        var baseAddressString = serverConfig?.Port is null ? serverConfig?.Host : $"{serverConfig.Host}:{serverConfig.Port}";
        
        _client = new HttpClient()
        {
            BaseAddress = new Uri(baseAddressString),
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json) },
                Authorization = new AuthenticationHeaderValue("Basic", userId)
            }
        };
    }
}