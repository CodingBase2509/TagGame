using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Abstractions.Rooms;
using TagGame.Api.Core.Common.Http;
using TagGame.Api.Core.Common.Security;
using TagGame.Api.Filters;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.DTOs.Rooms;
using TagGame.Shared.DTOs.Settings;

namespace TagGame.Api.Endpoints;

public class RoomsModule : EndpointBase, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var rooms = app.MapV1()
            .MapGroup("/rooms")
            .WithTags("rooms")
            .RequireAuthorization();

        rooms.MapGet("/{id:guid}/settings", GetRoomSettingsAsync)
            .WithName("Rooms_GetSettings")
            .Produces<RoomSettingsResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status304NotModified)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .AddEndpointFilter<RoomMembershipFilter>()
            .RequireAuthorization(AuthPolicyPrefix.RoomMember)
            .IncludeInOpenApi();

        rooms.MapPatch("/{id:guid}/settings", UpdateRoomSettingsAsync)
            .WithName("Rooms_PatchSettings")
            .Accepts<PatchRoomSettingsRequestDto>(MediaTypeNames.Application.Json)
            .Produces<RoomSettingsResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity, MediaTypeNames.Application.Json)
            .AddEndpointFilter<RoomMembershipFilter>()
            .RequireAuthorization(AuthPolicyPrefix.RoomMember)
            .RequireAuthorization(AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.EditSettings))
            .IncludeInOpenApi();

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

    private static async Task<IResult> GetRoomSettingsAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "If-None-Match")] string? ifNoneMatch,
        [FromServices] IRoomsService rooms,
        [FromServices] IHttpContextAccessor httpAccessor,
        CancellationToken ct)
    {
        var http = httpAccessor.HttpContext;

        var result = await rooms.GetSettingsWithTokenAsync(id, ct);
        if (!result.HasValue)
            return NotFound("Errors.Rooms.NotFound", "rooms.not_found");

        var decision = EtagUtils.CheckIfNoneMatch(ifNoneMatch, result.Value.Token);
        switch (decision)
        {
            case IfNoneMatchDecision.InvalidIfNoneMatch:
                return BadRequest("Errors.Http.InvalidIfNoneMatch", "invalid.if_none_match");
            case IfNoneMatchDecision.NotModified:
                http?.Response.SetEtag(result.Value.Token);
                return NotModified();
            case IfNoneMatchDecision.Proceed:
            default:
                http?.Response.SetEtag(result.Value.Token);
                return Ok(new RoomSettingsResponseDto
                {
                    HideTimeSec = result.Value.Settings.HideTimeSec,
                    HuntTimeSec = result.Value.Settings.HuntTimeSec,
                    TagRadiusM = result.Value.Settings.TagRadiusM
                });
        }
    }

    private static async Task<IResult> UpdateRoomSettingsAsync(
        [FromRoute] Guid id,
        [FromBody] PatchRoomSettingsRequestDto request,
        [FromServices] IRoomsService rooms,
        [FromServices] IValidator<PatchRoomSettingsRequestDto> validator,
        [FromServices] IHttpContextAccessor httpAccessor,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken ct)
    {
        var http = httpAccessor.HttpContext;
        if (string.IsNullOrWhiteSpace(ifMatch))
            return PreconditionRequired("Errors.Http.IfMatchRequired", "missing.if-match");

        await validator.ValidateAndThrowAsync(request, ct);

        var token = await rooms.GetRoomConcurrencyTokenAsync(id, ct);
        if (token is null)
            return NotFound("Errors.Rooms.NotFound", "rooms.not_found");

        var result = EtagUtils.CheckIfMatch(ifMatch, token.Value);
        switch (result)
        {
            case IfMatchCheckResult.MissingIfMatch:
            case IfMatchCheckResult.InvalidIfMatch:
                return BadRequest("Errors.Http.InvalidIfMatch", "invalid.if-match");
            case IfMatchCheckResult.EtagMismatch:
                http?.Response.SetEtag(token.Value);
                return PreconditionFailed("Errors.Http.IfMatchMismatch", "mismatch.if-match");
            case IfMatchCheckResult.Ok:
            case IfMatchCheckResult.Wildcard:
            default:
                break;
        }

        var settings =
            await rooms.UpdateSettingsAsync(id, request.HideTimeSec, request.HuntTimeSec, request.TagRadiusM, ct);
        if (settings is null)
            return NotFound("Errors.Rooms.NotFound", "rooms.not_found");

        try
        {
            await rooms.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            var errorToken = await rooms.GetRoomConcurrencyTokenAsync(id, ct);
            if (errorToken is not null)
                http?.Response.SetEtag(errorToken.Value);
            return PreconditionFailed("Errors.Http.IfMatchMismatch", "mismatch.if-match");
        }

        var newToken = await rooms.GetRoomConcurrencyTokenAsync(id, ct);
        if (newToken is not null)
            http?.Response.SetEtag(newToken.Value);
        return Ok(new RoomSettingsResponseDto
        {
            HideTimeSec = settings.HideTimeSec,
            HuntTimeSec = settings.HuntTimeSec,
            TagRadiusM = settings.TagRadiusM
        });
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
