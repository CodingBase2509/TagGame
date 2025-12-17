using TagGame.Api.Core.Abstractions.Rooms;
using TagGame.Api.Core.Common.Security;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Api.Endpoints;

public class RoomsModule : EndpointBase, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var rooms = app.MapV1()
            .MapGroup("/rooms")
            .WithTags("rooms")
            .RequireAuthorization();

        rooms.MapPost("/", CreateRoomAsync)
            .WithName("Lobby_CreateRoom")
            .Accepts<CreateRoomRequestDto>(MediaTypeNames.Application.Json)
            .Produces<CreateRoomResponseDto>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .IncludeInOpenApi();

        rooms.MapPost("/join", JoinRoomAsync)
            .WithName("Lobby_JoinRoom")
            .Accepts<JoinRoomRequestDto>(MediaTypeNames.Application.Json)
            .Produces<JoinRoomResponseDto>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .IncludeInOpenApi();
    }

    private static async Task<IResult> CreateRoomAsync(
        [FromBody] CreateRoomRequestDto request,
        [FromServices] IValidator<CreateRoomRequestDto> validator,
        [FromServices] IRoomsService rooms,
        [FromServices] IHttpContextAccessor httpAccessor,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        ArgumentNullException.ThrowIfNull(httpAccessor.HttpContext);

        if (!AuthUtils.TryGetUserId(httpAccessor.HttpContext, out var userId))
            return Unauthorized("Errors.Auth.MissingSub", "auth.missing_sub");

        var room = await rooms.CreateRoomAsync(userId, request.Name.Trim(), ct);
        var membership = await rooms.CreateMembershipAsync(
            userId,
            room.Id,
            RoomRole.Owner,
            PermissionProfiles.OwnerMask,
            PlayerType.Hider,
            ct);

        await rooms.SaveChangesAsync(ct);

        return Created("rooms", new CreateRoomResponseDto
        {
            RoomId = room.Id,
            Name = room.Name,
            MembershipId = membership.Id
        });
    }

    private static async Task<IResult> JoinRoomAsync(
        [FromBody] JoinRoomRequestDto request,
        [FromServices] IValidator<JoinRoomRequestDto> validator,
        [FromServices] IRoomsService rooms,
        [FromServices] IHttpContextAccessor httpAccessor,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        ArgumentNullException.ThrowIfNull(httpAccessor.HttpContext);

        if (!AuthUtils.TryGetUserId(httpAccessor.HttpContext, out var userId))
            return Unauthorized("Errors.Auth.MissingSub", "auth.missing_sub");

        var room = await rooms.GetRoomByAccessCodeAsync(request.AccessCode.Trim(), ct);
        if (room is null)
            return NotFound("Errors.Rooms.NotFound", "rooms.not_found");

        var membership = await rooms.GetMembershipAsync(userId, room.Id, ct);
        if (membership is not null)
        {
            if (membership.IsBanned)
                return Forbidden("Errors.Rooms.Banned", "rooms.banned");
            else
                return Ok(new JoinRoomResponseDto
                {
                    Name = room.Name,
                    RoomId = room.Id,
                    MembershipId = membership.Id
                });
        }

        membership = await rooms.CreateMembershipAsync(
            userId,
            room.Id,
            RoomRole.Player,
            PermissionProfiles.PlayerMask,
            PlayerType.Hider,
            ct);

        await rooms.SaveChangesAsync(ct);

        return Ok(new JoinRoomResponseDto
        {
            RoomId = room.Id,
            Name = room.Name,
            MembershipId = membership.Id
        });
    }
}
