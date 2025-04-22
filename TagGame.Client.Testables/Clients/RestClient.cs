using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.DTOs.Users;

[assembly: InternalsVisibleTo("TagGame.Client.Tests")]
namespace TagGame.Client.Clients;

public class RestClient(ConfigHandler configHandler, IOptions<JsonSerializerOptions> jsonOptions)
{
    private HttpClient? _client;

    internal RestClient(ConfigHandler configHandler, HttpClient client, IOptions<JsonSerializerOptions> jsonOptions)
        : this(configHandler, jsonOptions)
    {
        _client = client;
    }
    
    public async Task<Response<CreateGameRoom.CreateGameRoomResponse>?> CreateRoomAsync(CreateGameRoom.CreateGameRoomRequest createGameRoomRequest)
    {
        try
        {
            await InitAsync();

            var stringContent = JsonSerializer.Serialize(createGameRoomRequest, jsonOptions.Value);
            var response = await _client!.PostAsync(ApiRoutes.GameRoom.GroupName 
                                                    + ApiRoutes.GameRoom.CreateRoom,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<CreateGameRoom.CreateGameRoomResponse>>(content, jsonOptions.Value);
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

            var stringContent = JsonSerializer.Serialize(joinGameRoomRequest, jsonOptions.Value);
            var response = await _client!.PostAsync(ApiRoutes.GameRoom.GroupName 
                                                    + ApiRoutes.GameRoom.JoinRoom.Replace("{roomId:guid}", Guid.Empty.ToString()),
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<JoinGameRoom.JoinGameRoomResponse>>(content, jsonOptions.Value);
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
            var response = await _client!.GetAsync(route);
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<GameRoom>>(content, jsonOptions.Value);
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
        
            var stringContent = JsonSerializer.Serialize(settings, jsonOptions.Value);
            var response = await _client!.PutAsync(ApiRoutes.GameRoom.GroupName 
                                                   + ApiRoutes.GameRoom.UpdateSettings
                                                       .Replace("{roomId:guid}", settings.RoomId.ToString()),
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Response<string>>(content, jsonOptions.Value);
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

            var stringContent = JsonSerializer.Serialize(createUserRequest, jsonOptions.Value);
            var response = await _client!.PostAsync(ApiRoutes.Initial.CreateUser,
                new StringContent(stringContent, Encoding.UTF8, MediaTypeNames.Application.Json));
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Response<User>>(content, jsonOptions.Value);
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

        var baseAddressChanged = BaseAddressHasChanged(
            serverConfig?.Host ?? string.Empty,
            serverConfig?.Port,
            out var newBaseAddress);
        var userIdChanged = UserIdHasChanged(userConfig?.UserId.ToString());

        if (_client is not null && !baseAddressChanged && !userIdChanged)
            return;
        
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert is { Issuer: "CN=localhost" })
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
#endif


        var idString = userConfig?.UserId.ToString();
        var b64Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(idString));
        
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(newBaseAddress),
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json) },
                Authorization = new AuthenticationHeaderValue("Basic", b64Id)
            }
        };
    }

    private bool BaseAddressHasChanged(string host, int? port, out string baseAddress)
    {
        var currentBaseAddress = _client?.BaseAddress!.AbsoluteUri.TrimEnd('/');
        baseAddress = port is null ? host : $"{host}:{port}";
        
        return !Equals(baseAddress, currentBaseAddress);
    }

    private bool UserIdHasChanged(string? userId)
    {
        var headerId = _client?.DefaultRequestHeaders.Authorization?.Parameter;
        
        var plainTextBytes = Encoding.UTF8.GetBytes(userId ?? string.Empty);
        var configId = Convert.ToBase64String(plainTextBytes);

        return !Equals(headerId, configId);
    }
}