using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TagGame.Api.Filters;

namespace TagGame.Api.Tests.Integration.Games.TestModules;

/// <summary>
/// Carter test module exposing minimal endpoints to verify dynamic policies and membership filter.
/// Routes are under /v1/_it/rooms/{roomId}/...
/// </summary>
public sealed class AuthZTestModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var root = app.MapGroup("/v1/_it/rooms/{roomId:guid}");

        // Unfiltered endpoints (policy handlers load membership lazily from DB)
        var unfiltered = root.MapGroup("/unfiltered");
        unfiltered.MapGet("/member/probe", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomMember");

        unfiltered.MapGet("/perm/start", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomPermission:StartGame");

        unfiltered.MapGet("/role/owner", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomRole:Owner");

        // Filtered endpoints (membership must exist and not be banned; stored in HttpContext.Items)
        var filtered = root.MapGroup("/filtered");
        filtered.AddEndpointFilter(new RoomMembershipFilter());
        filtered.MapGet("/probe", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomMember");

        filtered.MapGet("/perm/start", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomPermission:StartGame");

        filtered.MapGet("/role/owner", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RoomRole:Owner");
    }
}
