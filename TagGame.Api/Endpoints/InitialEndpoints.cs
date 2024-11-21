using System.Net.Mime;
using Carter;
using Microsoft.AspNetCore.Mvc;
using TagGame.Api.Services;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Endpoints;

public class InitialEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoutes.Initial.CreateUser, CreateUserAsync)
            .Accepts<CreateUser.Request>(MediaTypeNames.Application.Json)
            .Produces<Response<User>>(StatusCodes.Status200OK)
            .Produces<Response<User>>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();
    }

    public async Task<Response<User>> CreateUserAsync([FromServices] PlayerService player, CreateUser.Request request)
    {
        var user = await player.AddUserAsync(request.Name, request.AvatarColor);
        if (user is null)
            return new Error(500, "not-created-user");

        return user;
    }
}