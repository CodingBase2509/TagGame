using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq.Protected;
using TagGame.Client.Clients;
using TagGame.Client.Common;
using TagGame.Client.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Games;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Client.Tests.Unit.Clients;

public class RestClientTests : TestBase
{
    private readonly Mock<ConfigHandler> _configHandlerMock;
    private readonly Fixture _fixture;
    private readonly RestClient _restClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public RestClientTests()
    {
        _fixture = new Fixture();
        _configHandlerMock = new Mock<ConfigHandler>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        var userId = InitClient();
        var base64Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString())) ?? string.Empty;
        
        var jsonOptions = new Mock<IOptions<JsonSerializerOptions>>();
        jsonOptions.Setup(x => x.Value)
            .Returns(() =>
            {
                MappingOptions.JsonSerializerOptions.Converters.Add(new MauiColorJsonConverter());
                return MappingOptions.JsonSerializerOptions;
            });
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://127.0.0.1:5000")
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Id);
        _restClient = new RestClient(_configHandlerMock.Object, httpClient, jsonOptions.Object);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldReturnResponse_WhenSuccessful()
    {
        // Arrange
        var request = _fixture.Create<CreateGameRoom.CreateGameRoomRequest>();
        var expectedResponse = new Response<CreateGameRoom.CreateGameRoomResponse>()
        {
            IsSuccess = true,
            Value = _fixture.Build<CreateGameRoom.CreateGameRoomResponse>()
                .With(x => x.RoomName, request.GameRoomName)
                .Create(),
            Error = null
        };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _restClient.CreateRoomAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.RoomName.Should().Be(expectedResponse.Value.RoomName);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldThrowException_WhenHttpRequestFails()
    {
        // Arrange
        var request = _fixture.Create<CreateGameRoom.CreateGameRoomRequest>();
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _restClient.CreateRoomAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("error-sending-request");
    }

    [Fact]
    public async Task JoinRoomAsync_ShouldReturnResponse_WhenSuccessful()
    {
        var request = _fixture.Create<JoinGameRoom.JoinGameRoomRequest>();
        var expectedResponse = _fixture.Create<Response<JoinGameRoom.JoinGameRoomResponse>>();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _restClient.JoinRoomAsync(request);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task JoinRoomAsync_ShouldThrowException_WhenHttpRequestFails()
    {
        // Arrange
        var request = _fixture.Create<JoinGameRoom.JoinGameRoomRequest>();
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _restClient.JoinRoomAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("error-sending-request");
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnResponse_WhenSuccessful()
    {
        var roomId = Guid.NewGuid();
        var expectedResponse = _fixture.Create<Response<GameRoom>>();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _restClient.GetRoomAsync(roomId);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldThrowException_WhenHttpRequestFails()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _restClient.GetRoomAsync(roomId);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("error-sending-request");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnResponse_WhenSuccessful()
    {
        var request = _fixture.Create<CreateUser.CreateUserRequest>();
        var expectedResponse = _fixture.Create<Response<User>>();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _restClient.CreateUserAsync(request);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowException_WhenHttpRequestFails()
    {
        // Arrange
        var request = _fixture.Create<CreateUser.CreateUserRequest>();
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _restClient.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("error-sending-request");
    }

    private Guid InitClient()
    {
        var serverConfig = new ServerConfig()
        {
            Host = "http://127.0.0.1",
            Port = 5000,
        };
        var userConfig = _fixture.Create<UserConfig>();
        
        _configHandlerMock.Setup(x => x.ReadAsync<ServerConfig>()).ReturnsAsync(serverConfig);
        _configHandlerMock.Setup(x => x.ReadAsync<UserConfig>()).ReturnsAsync(userConfig);
        
        return userConfig.UserId;
    }
    
    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "", string method = "SendAsync")
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(method, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }
}