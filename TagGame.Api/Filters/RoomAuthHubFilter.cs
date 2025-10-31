using System.Reflection;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Filters;

public class RoomAuthHubFilter(
    IGamesUoW gamesUoW,
    IAuthorizationPolicyProvider policyProvider,
    IAuthorizationService authorizationService,
    ILogger<RoomAuthHubFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        // 1) Identity present?
        var user = invocationContext.Context.User;
        if (user?.Identity?.IsAuthenticated != true)
            throw new HubException("auth.invalid_token");

        // 2) Extract roomId (first Guid argument or DTO with RoomId property)
        var roomId = GetRoomId(invocationContext);

        // 3) Extract userId from claims
        if (!TryGetUserId(user, out var userId))
            throw new HubException("auth.invalid_token");

        // 4) Load membership and basic guards
        var ct = invocationContext.Context.ConnectionAborted;
        var membership = await AuthUtils.LoadMembershipAsync(gamesUoW, userId, roomId, ct);
        EnsureValidMembership(membership);

        // 5) Stash for handlers and downstream hub logic
        invocationContext.Context.Items["RoomId"] = roomId;
        invocationContext.Context.Items["Membership"] = membership;

        // 6) Build combined policy from [Authorize] on Hub type + method
        var authorizeData = GetAuthorizeData(invocationContext);
        if (authorizeData.Count <= 0)
            return await next(invocationContext);

        var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
        if (policy is null)
            return await next(invocationContext);

        var result = await authorizationService.AuthorizeAsync(user, resource: invocationContext, policy);
        if (result.Succeeded)
            return await next(invocationContext);

        logger.LogDebug("Hub auth failed for {Hub}.{Method} with policy {Policy}",
            invocationContext.Hub.GetType().Name, invocationContext.HubMethodName, string.Join(',', authorizeData.Select(a => a.Policy)));
        throw new HubException("auth.missing_permission");
    }

    private static void EnsureValidMembership(RoomMembership? membership)
    {
        if (membership is null)
            throw new HubException("auth.not_member");

        if (membership.IsBanned)
            throw new HubException("auth.banned");
    }

    private static Guid GetRoomId(HubInvocationContext context)
    {
        // 1) Prefer first Guid argument
        foreach (var arg in context.HubMethodArguments)
        {
            switch (arg)
            {
                case Guid g:
                    return g;
                case string s when Guid.TryParse(s, out var gs):
                    return gs;
                default:
                    break;
            }
        }

        // 2) Try DTO with RoomId property (case-insensitive)
        foreach (var arg in context.HubMethodArguments)
        {
            if (arg is null) continue;
            var roomIdProp = arg.GetType().GetProperty("RoomId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var value = roomIdProp?.GetValue(arg);
            switch (value)
            {
                case Guid g:
                    return g;
                case string s when Guid.TryParse(s, out var gs):
                    return gs;
                default:
                    break;
            }
        }

        throw new HubException("auth.room_id_missing");
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out userId);
    }

    private static List<IAuthorizeData> GetAuthorizeData(HubInvocationContext context)
    {
        var list = new List<IAuthorizeData>();
        var hubType = context.Hub.GetType();
        list.AddRange(hubType.GetCustomAttributes(true).OfType<IAuthorizeData>());

        var candidates = hubType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == context.HubMethodName)
            .ToList();
        var method = candidates.Count switch
        {
            1 => candidates[0],
            > 1 => candidates.FirstOrDefault(m => m.GetParameters().Length == context.HubMethodArguments.Count) ??
                   candidates[0],
            _ => null
        };

        if (method is not null)
            list.AddRange(method.GetCustomAttributes(true).OfType<IAuthorizeData>());

        return list;
    }
}
