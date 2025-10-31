using Microsoft.Extensions.Options;
using TagGame.Api.Infrastructure.Auth.Requirements;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Infrastructure.Auth;

public class AuthPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrEmpty(policyName))
            return base.GetPolicyAsync(policyName);

        var pb = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser();

        if (IsRoomPermission(policyName))
        {
            var name = policyName[AuthPolicyPrefix.RoomPermission.Length..];
            if (TryParse<RoomPermission>(name, out var permission))
            {
                pb.AddRequirements(new RoomMemberRequirement());
                pb.AddRequirements(new RoomPermissionRequirement(permission));
                return Task.FromResult<AuthorizationPolicy?>(pb.Build());
            }
        }

        if (IsRoomRole(policyName))
        {
            var name = policyName[AuthPolicyPrefix.RoomRole.Length..];
            if (TryParse<RoomRole>(name, out var role))
            {
                pb.AddRequirements(new RoomMemberRequirement());
                pb.AddRequirements(new RoomRoleRequirement(role));
                return Task.FromResult<AuthorizationPolicy?>(pb.Build());
            }
        }

        if (!IsRoomMember(policyName))
            return base.GetPolicyAsync(policyName);

        pb.AddRequirements(new RoomMemberRequirement());
        return Task.FromResult<AuthorizationPolicy?>(pb.Build());

    }

    private static bool IsRoomPermission(string policyName) =>
        policyName.StartsWith(AuthPolicyPrefix.RoomPermission, StringComparison.Ordinal);

    private static bool IsRoomRole(string policyName) =>
        policyName.StartsWith(AuthPolicyPrefix.RoomRole, StringComparison.Ordinal);

    private static bool IsRoomMember(string policyName) =>
        policyName.Equals(AuthPolicyPrefix.RoomMember, StringComparison.Ordinal);

    private static bool TryParse<TEnum>(string name, out TEnum value) where TEnum : struct
    {
        var parsed = Enum.TryParse<TEnum>(name, ignoreCase: true, out var enumValue);
        value = enumValue;
        return parsed;
    }
}
