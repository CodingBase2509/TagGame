using System.Net.Mime;
using TagGame.Shared.Domain.Games;
using Carter;
using TagGame.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using TagGame.Shared.DTOs.Games;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Common;
using TagGame.Api.Validation.GameRoom;

namespace TagGame.Api.Endpoints;

public class GameRoomEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gameroom = app.MapGroup(ApiRoutes.GameRoom.GroupName);

        gameroom.MapPost(ApiRoutes.GameRoom.CreateRoom, CreateRoom)
            .Accepts<CreateGameRoom.CreateGameRoomRequest>(MediaTypeNames.Application.Json)
            .Produces<Response<CreateGameRoom.CreateGameRoomResponse>>(StatusCodes.Status201Created)
            .Produces<Response<Error>>(StatusCodes.Status400BadRequest)
            .Produces<Response<Error>>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous()
            .WithOpenApi();

        gameroom.MapPost(ApiRoutes.GameRoom.JoinRoom, JoinRoom)
            .Accepts<JoinGameRoom.JoinGameRoomRequest>(MediaTypeNames.Application.Json)
            .Produces<Response<JoinGameRoom.JoinGameRoomResponse>>(StatusCodes.Status201Created)
            .Produces<Response<Error>>(StatusCodes.Status400BadRequest)
            .Produces<Response<Error>>(StatusCodes.Status404NotFound)
            .Produces<Response<Error>>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous()
            .WithOpenApi();

        gameroom.MapGet(ApiRoutes.GameRoom.GetRoom, GetRoom)
            .Produces<Response<GameRoom>>(StatusCodes.Status200OK)
            .Produces<Response<Error>>(StatusCodes.Status400BadRequest)
            .Produces<Response<Error>>(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithOpenApi();

        gameroom.MapPut(ApiRoutes.GameRoom.UpdateSettings, UpdateSettings)
            .Accepts<Response<string>>(MediaTypeNames.Application.Json)
            .Produces<Response<Error>>(StatusCodes.Status400BadRequest)
            .Produces<Response<Error>>(StatusCodes.Status404NotFound)
            .Produces<Response<Error>>(StatusCodes.Status500InternalServerError)
            .RequireAuthorization()
            .WithOpenApi();
    }

    public async Task<IResult> CreateRoom(
        [FromServices] GameRoomService roomService,
        [FromServices] PlayerService playerService,
        [FromServices] CreateGameRoomValidator validator,
        CreateGameRoom.CreateGameRoomRequest createGameRoomRequest)
    {
        var validationResult = await validator.ValidateAsync(createGameRoomRequest);
        if (!validationResult.IsValid)
            return new Error(400, validationResult)
                .ToHttpResult();

        var room = await roomService.CreateAsync(createGameRoomRequest.UserId, createGameRoomRequest.GameRoomName);
        if (room is null)
            return new Error(500, "not-created-room")
                .ToHttpResult();
        
        var player = await playerService.CreatePlayerAsync(createGameRoomRequest.UserId);
        if (player is null)
            return new Error(500, "not-created-player")
                .ToHttpResult();

        var success = await playerService.AddPlayerToRoomAsync(room.Id, player.Id);
        if (!success)
            return new Error(500, "player-not-joined-room")
                .ToHttpResult();

        return new CreateGameRoom.CreateGameRoomResponse()
        {
            RoomId = room.Id,
            AccessCode = room.AccessCode,
            RoomName = room.Name,
        }.ToHttpResult();
    }

    public async Task<IResult> JoinRoom(
        [FromServices] GameRoomService roomService,
        [FromServices] PlayerService playerService,
        [FromServices] JoinRoomValidator validator,
        JoinGameRoom.JoinGameRoomRequest joinGameRoomRequest)
    {
        var validationResult = await validator.ValidateAsync(joinGameRoomRequest);
        if (!validationResult.IsValid)
            return new Error(400, validationResult)
                .ToHttpResult();

        var room = await roomService.GetRoomAsync(joinGameRoomRequest.GameName, joinGameRoomRequest.AccessCode);
        if (room is null)
            return new Error(404, "not-found-room")
                .ToHttpResult();

        var player = await playerService.CreatePlayerAsync(joinGameRoomRequest.UserId);
        if (player is null)
            return new Error(500, "not-created-player")
                .ToHttpResult();

        var addSuccess = await playerService.AddPlayerToRoomAsync(room.Id, player.Id);
        if (!addSuccess)
            return new Error(500, "player-not-joined-room")
                .ToHttpResult();

        return new JoinGameRoom.JoinGameRoomResponse()
        {
            Room = room,
            Player = player,
        }.ToHttpResult();
    }

    public async Task<IResult> GetRoom([FromServices] GameRoomService roomService, Guid roomId)
    {
        var room = await roomService.GetRoomAsync(roomId);
        if (room is null)
            return new Error(404, "not-found-room")
                .ToHttpResult();
        
        return room
            .ToHttpResult();
    }

    public async Task<IResult> UpdateSettings(
        [FromServices] GameRoomService roomService,
        [FromServices] GameRoomSettingsValidator validator,
        Guid roomId,
        [FromBody] GameSettings settings)
    {
        var validationResult = await validator.ValidateAsync(settings);
        if (!validationResult.IsValid)
            return new Error(400, validationResult)
                .ToHttpResult();
        
        var success = await roomService.UpdateSettingsAsync(roomId, settings);

        return !success ? 
            new Error(500, "settings-invalid")
                .ToHttpResult()
            : Response<string>.Empty
                .ToHttpResult();
    }
}
