using Microsoft.AspNetCore.Authorization;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Infrastructure.Auth.Requirements;

public sealed class RoomPermissionRequirement(RoomPermission permission) : IAuthorizationRequirement
{
    public RoomPermission Permission { get; } = permission;
}
