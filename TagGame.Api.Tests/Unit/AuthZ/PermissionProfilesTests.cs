using FluentAssertions;
using TagGame.Api.Core.Common.Security;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Tests.Unit.AuthZ;

public sealed class PermissionProfilesTests
{
    [Fact]
    public void OwnerMask_contains_all_permissions()
    {
        var mask = PermissionProfiles.OwnerMask;

        mask.Includes(RoomPermission.StartGame).Should().BeTrue();
        mask.Includes(RoomPermission.EditSettings).Should().BeTrue();
        mask.Includes(RoomPermission.Invite).Should().BeTrue();
        mask.Includes(RoomPermission.KickPlayer).Should().BeTrue();
        mask.Includes(RoomPermission.Tag).Should().BeTrue();
        mask.Includes(RoomPermission.ManageRoles).Should().BeTrue();
    }

    [Fact]
    public void ModeratorMask_contains_operational_permissions_but_not_manage_roles()
    {
        var mask = PermissionProfiles.ModeratorMask;

        mask.Includes(RoomPermission.ManageRoles).Should().BeFalse();
        mask.Includes(RoomPermission.StartGame).Should().BeTrue();
        mask.Includes(RoomPermission.EditSettings).Should().BeTrue();
        mask.Includes(RoomPermission.Invite).Should().BeTrue();
        mask.Includes(RoomPermission.KickPlayer).Should().BeTrue();
        mask.Includes(RoomPermission.Tag).Should().BeTrue();
    }

    [Fact]
    public void PlayerMask_is_minimal()
    {
        var mask = PermissionProfiles.PlayerMask;

        mask.Should().Be(RoomPermission.Tag);
        mask.Includes(RoomPermission.StartGame).Should().BeFalse();
        mask.Includes(RoomPermission.EditSettings).Should().BeFalse();
        mask.Includes(RoomPermission.Invite).Should().BeFalse();
        mask.Includes(RoomPermission.KickPlayer).Should().BeFalse();
        mask.Includes(RoomPermission.ManageRoles).Should().BeFalse();
    }

    [Fact]
    public void Includes_and_IncludesAny_behave_as_expected()
    {
        var current = RoomPermission.StartGame | RoomPermission.Invite;

        current.Includes(RoomPermission.StartGame).Should().BeTrue();
        current.Includes(RoomPermission.StartGame | RoomPermission.EditSettings).Should().BeFalse();

        current.IncludesAny(RoomPermission.EditSettings | RoomPermission.Invite).Should().BeTrue();
        current.IncludesAny(RoomPermission.EditSettings | RoomPermission.KickPlayer).Should().BeFalse();

        RoomPermission.None.Includes(RoomPermission.None).Should().BeTrue();
        RoomPermission.None.IncludesAny(RoomPermission.None).Should().BeFalse();
    }

    [Fact]
    public void Effective_combines_base_grants_and_denies()
    {
        var eff1 = PermissionExtensions.Effective(RoomRole.Player, granted: RoomPermission.Invite);
        eff1.Includes(RoomPermission.Invite).Should().BeTrue();
        eff1.Includes(RoomPermission.Tag).Should().BeTrue();

        var eff2 = PermissionExtensions.Effective(
            RoomRole.Player,
            granted: RoomPermission.Invite,
            denied: RoomPermission.Tag);
        eff2.Should().Be(RoomPermission.Invite);

        var eff3 = PermissionExtensions.Effective(
            RoomRole.Moderator,
            denied: RoomPermission.EditSettings | RoomPermission.KickPlayer);
        eff3.Includes(RoomPermission.StartGame).Should().BeTrue();
        eff3.Includes(RoomPermission.EditSettings).Should().BeFalse();
        eff3.Includes(RoomPermission.KickPlayer).Should().BeFalse();
    }

    [Fact]
    public void GetMask_returns_expected_per_role()
    {
        PermissionProfiles.GetMask(RoomRole.Owner).Should().Be(PermissionProfiles.OwnerMask);
        PermissionProfiles.GetMask(RoomRole.Moderator).Should().Be(PermissionProfiles.ModeratorMask);
        PermissionProfiles.GetMask(RoomRole.Player).Should().Be(PermissionProfiles.PlayerMask);
    }
}

