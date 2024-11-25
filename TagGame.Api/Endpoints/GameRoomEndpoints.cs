﻿using System.Net.Mime;
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
            .Produces<Response<CreateGameRoom.CreateGameRoomResponse>>(StatusCodes.Status400BadRequest)
            .Produces<Response<CreateGameRoom.CreateGameRoomResponse>>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous()
            .WithOpenApi();

        gameroom.MapPost(ApiRoutes.GameRoom.JoinRoom, JoinRoom)
            .Accepts<JoinGameRoom.JoinGameRoomRequest>(MediaTypeNames.Application.Json)
            .Produces<Response<JoinGameRoom.JoinGameRoomResponse>>(StatusCodes.Status201Created)
            .Produces<Response<JoinGameRoom.JoinGameRoomResponse>>(StatusCodes.Status400BadRequest)
            .Produces<Response<JoinGameRoom.JoinGameRoomResponse>>(StatusCodes.Status404NotFound)
            .Produces<Response<JoinGameRoom.JoinGameRoomResponse>>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous()
            .WithOpenApi();

        gameroom.MapGet(ApiRoutes.GameRoom.GetRoom, GetRoom)
            .Produces<Response<GameRoom>>(StatusCodes.Status200OK)
            .Produces<Response<GameRoom>>(StatusCodes.Status400BadRequest)
            .Produces<Response<GameRoom>>(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithOpenApi();

        gameroom.MapPut(ApiRoutes.GameRoom.UpdateSettings, UpdateSettings)
            .Accepts<Response>(MediaTypeNames.Application.Json)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .RequireAuthorization()
            .WithOpenApi();
    }

    public async Task<Response<CreateGameRoom.CreateGameRoomResponse>> CreateRoom(
        [FromServices] GameRoomService roomService,
        [FromServices] PlayerService playerService,
        [FromServices] CreateGameRoomValidator validator,
        CreateGameRoom.CreateGameRoomRequest createGameRoomRequest)
    {
        if (createGameRoomRequest is null)
            return new Error(400, "request-null");

        var validationResult = await validator.ValidateAsync(createGameRoomRequest);
        if (!validationResult.IsValid)
            return new Error(400, validationResult);

        var room = await roomService.CreateAsync(createGameRoomRequest.UserId, createGameRoomRequest.GameRoomName);
        if (room is null)
            return new Error(500, "not-created-room");
        
        var player = await playerService.CreatePlayerAsync(createGameRoomRequest.UserId);
        if (player is null)
            return new Error(500, "not-created-player");

        var success = await playerService.AddPlayerToRoomAsync(room.Id, player.Id);
        if (!success)
            return new Error(500, "player-not-joined-room");

        return new CreateGameRoom.CreateGameRoomResponse()
        {
            RoomId = room.Id,
            AccessCode = room.AccessCode,
            RoomName = room.Name,
        };
    }

    public async Task<Response<JoinGameRoom.JoinGameRoomResponse>> JoinRoom(
        [FromServices] GameRoomService roomService,
        [FromServices] PlayerService playerService,
        [FromServices] JoinRoomValidator validator,
        JoinGameRoom.JoinGameRoomRequest joinGameRoomRequest)
    {
        if (joinGameRoomRequest is null)
            return new Error(400, "request-null");

        var validationResult = await validator.ValidateAsync(joinGameRoomRequest);
        if (!validationResult.IsValid)
            return new Error(400, validationResult);

        var room = await roomService.GetRoomAsync(joinGameRoomRequest.GameName, joinGameRoomRequest.AccessCode);
        if (room is null)
            return new Error(404, "not-found-room");

        var player = await playerService.CreatePlayerAsync(joinGameRoomRequest.UserId);
        if (player is null)
            return new Error(500, "not-created-player");

        var addSuccess = await playerService.AddPlayerToRoomAsync(player.Id, room.Id);
        if (!addSuccess)
            return new Error(500, "player-not-joined-room");

        return new JoinGameRoom.JoinGameRoomResponse()
        {
            Room = room,
            Player = player,
        };
    }

    public async Task<Response<GameRoom?>> GetRoom([FromServices] GameRoomService roomService, Guid roomId)
    {
        var room = await roomService.GetRoomAsync(roomId);
        if (room is null)
            return new Error(404, "not-found-room");
        
        return room;
    }

    public async Task<Response> UpdateSettings(
        [FromServices] GameRoomService roomService,
        [FromServices] GameRoomSettingsValidator validator,
        Guid roomId,
        [FromBody] GameSettings settings)
    {
        var validationResult = await validator.ValidateAsync(settings);
        if (!validationResult.IsValid)
            return new Error(400, validationResult);
        
        var success = await roomService.UpdateSettingsAsync(roomId, settings);

        return !success ? new Error(500, "settings-invalid") : Response.Empty;
    }
}
