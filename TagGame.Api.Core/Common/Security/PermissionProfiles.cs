using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Core.Common.Security;

public class PermissionProfiles
{
    public static RoomPermission OwnerMask => RoomPermission.StartGame
                                      | RoomPermission.EditSettings
                                      | RoomPermission.Invite
                                      | RoomPermission.KickPlayer
                                      | RoomPermission.Tag
                                      | RoomPermission.ManageRoles;

    public static RoomPermission ModeratorMask => RoomPermission.StartGame
                                           | RoomPermission.EditSettings
                                           | RoomPermission.Invite
                                           | RoomPermission.KickPlayer
                                           | RoomPermission.Tag;

    public static RoomPermission PlayerMask => RoomPermission.Tag;

    public static RoomPermission AllMask => OwnerMask;

    public static RoomPermission GetMask(RoomRole role) => role switch
    {
        RoomRole.Owner => OwnerMask,
        RoomRole.Moderator => ModeratorMask,
        RoomRole.Player => PlayerMask,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}
