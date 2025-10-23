using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.StartedAt).IsRequired();

        builder.HasIndex(m => m.RoomId);

        builder.HasOne<GameRoom>()
            .WithMany()
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
