using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Users;

[assembly: InternalsVisibleTo("TagGame.Client.Tests")]
namespace TagGame.Client.Clients;

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
        try
        {
            await InitAsync();

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
        try
        {
            await InitAsync();

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
        try
        {
            await InitAsync();

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
        try
        {
            await InitAsync();
        
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
        try
        {
            await InitAsync();

            var stringContent = JsonSerializer.Serialize(createUserRequest, MappingOptions.JsonSerializerOptions);
            var response = await _client.PostAsync(ApiRoutes.Initial.CreateUser,
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
        var serverConfig = await configHandler.ReadAsync<ServerConfig>();
        var userConfig = await configHandler.ReadAsync<UserConfig>();
        
        var baseAddressString = serverConfig?.Port is null ? serverConfig?.Host : $"{serverConfig.Host}:{serverConfig.Port}";
        if (_client is not null && Equals(baseAddressString, _client.BaseAddress!.ToString()))
            return;
        
        var plainTextBytes = Encoding.UTF8.GetBytes(userConfig?.UserId.ToString() ?? Guid.Empty.ToString());
        var userId = Convert.ToBase64String(plainTextBytes);

        if (_client is not null && Equals(userId, _client.DefaultRequestHeaders.Authorization?.ToString().Replace("Basic ", "")))
            return;
        
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Equals("CN=localhost"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
#endif
        
        _client = new HttpClient(handler)
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