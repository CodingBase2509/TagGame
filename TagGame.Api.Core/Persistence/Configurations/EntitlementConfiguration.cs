using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class EntitlementConfiguration : IEntityTypeConfiguration<Entitlement>
{
    public void Configure(EntityTypeBuilder<Entitlement> builder)
    {
        builder.ToTable("entitlements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.ExternalPayload)
            .HasColumnType("jsonb");

        builder.Property(e => e.ValidFrom).IsRequired();

        builder.HasIndex(e => e.UserId);

        // Ensure only one entitlement of a given type per user
        builder.HasIndex(e => new { e.UserId, e.Type })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
