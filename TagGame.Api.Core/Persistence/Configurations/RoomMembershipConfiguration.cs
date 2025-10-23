using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class RoomMembershipConfiguration : IEntityTypeConfiguration<RoomMembership>
{
    public void Configure(EntityTypeBuilder<RoomMembership> builder)
    {
        builder.ToTable("room_memberships");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role).IsRequired();
        builder.Property(m => m.PermissionsMask).IsRequired();
        builder.Property(m => m.JoinedAt).IsRequired();

        builder.HasIndex(m => new { m.UserId, m.RoomId })
            .IsUnique();

        builder.HasIndex(m => m.RoomId);

        // Relationship: Membership -> Room (FK)
        builder.HasOne<GameRoom>()
            .WithMany()
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
