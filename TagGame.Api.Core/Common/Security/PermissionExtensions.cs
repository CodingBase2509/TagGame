using System.Runtime.CompilerServices;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Core.Common.Security;

public static class PermissionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Includes(this RoomPermission current, RoomPermission required) =>
        (current & required) == required;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IncludesAny(this RoomPermission current, RoomPermission flags) =>
        (current & flags) != RoomPermission.None;

    public static RoomPermission Effective(
        RoomRole role,
        RoomPermission granted = RoomPermission.None,
        RoomPermission denied = RoomPermission.None)
    {
        var @base = PermissionProfiles.GetMask(role);
        return (@base | granted) & ~denied;
    }
}
