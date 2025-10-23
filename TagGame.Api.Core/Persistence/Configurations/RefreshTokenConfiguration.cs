using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.FamilyId);
        builder.HasIndex(t => t.ReplacedById);

        builder.HasIndex(t => t.TokenHash).IsUnique();

        // Relationship: RefreshToken -> User (FK), delete tokens when user is deleted
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
