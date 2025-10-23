using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("rounds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Phase).IsRequired();
        builder.Property(r => r.StartedAt).IsRequired();

        builder.HasIndex(r => r.MatchId);
        builder.HasIndex(r => new { r.MatchId, r.RoundNo }).IsUnique();

        builder.HasOne<Match>()
            .WithMany()
            .HasForeignKey(r => r.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
