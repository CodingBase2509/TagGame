using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DisplayName)
            .HasMaxLength(64);

        builder.Property(u => u.Email)
            .HasMaxLength(256);

        builder.Property(u => u.DeviceId)
            .HasMaxLength(64);

        builder.Property(u => u.AvatarColor)
            .HasMaxLength(9); // e.g. #RRGGBB or #AARRGGBB

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.DeviceId);
    }
}
