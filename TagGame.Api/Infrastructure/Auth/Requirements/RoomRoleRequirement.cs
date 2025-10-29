using Microsoft.AspNetCore.Authorization;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Infrastructure.Auth.Requirements;

public sealed class RoomRoleRequirement(RoomRole role) : IAuthorizationRequirement
{
    public RoomRole Role { get; } = role;
}
