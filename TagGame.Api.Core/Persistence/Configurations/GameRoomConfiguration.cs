using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class GameRoomConfiguration : IEntityTypeConfiguration<GameRoom>
{
    public void Configure(EntityTypeBuilder<GameRoom> builder)
    {
        builder.ToTable("game_rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.AccessCode)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(r => r.AccessCode)
            .IsUnique();

        builder.HasIndex(r => r.OwnerUserId);

        builder.Property(r => r.State)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Owned value object stored as JSON (PostgreSQL jsonb)
        builder.OwnsOne(r => r.Settings, nb =>
        {
            nb.ToJson();
        });

        // Optional polygon boundaries stored as JSON
        builder.OwnsOne(r => r.Boundaries, nb =>
        {
            nb.ToJson();
            // Map polygon points as JSON array of value objects
            nb.OwnsMany(p => p.Points);
        });
    }
}
