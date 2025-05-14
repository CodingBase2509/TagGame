using System.Drawing;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;
using TagGame.Shared.DTOs.Common;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Persistence;

public class GamesDbContext : DbContext
{
    internal DbSet<GameRoom> Rooms { get; set; }

    internal DbSet<GameSettings> Settings { get; set; }

    internal DbSet<Player> Players { get; set; }
    
    internal DbSet<User> Users { get; set; }
    
    internal DbSet<PlayerLeftGameInfo> PlayLeftInfo { get; set; }

    public GamesDbContext(DbContextOptions<GamesDbContext> options)
        : base(options)
    {
        // if (this.Database.IsRelational() && this.Database.GetPendingMigrations().Any())
        //     this.Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<GameRoom>(entity => 
        {
            entity.HasMany(r => r.Players)
            .WithOne();

            entity.HasIndex(r => r.Name);
            entity.HasIndex(r => r.AccessCode);
        });

        builder.Entity<GameSettings>(entity => 
        {
            entity.OwnsOne(s => s.Area, area => 
            {
                area.Property(a => a.Shape)
                    .HasColumnName("Area_Shape");
                area.Property(a => a.Boundary)
                    .HasColumnName("Area_Boundary")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, MappingOptions.JsonSerializerOptions),
                        v => JsonSerializer.Deserialize<Location[]>(v, MappingOptions.JsonSerializerOptions) ??
                             new Location[1])
                    .HasColumnType("jsonb");
            });
        });

        builder.Entity<Player>(entity => 
        {
            entity.Property(e => e.Location)
                .HasConversion(
                        v => JsonSerializer.Serialize(v, MappingOptions.JsonSerializerOptions),
                        v => JsonSerializer.Deserialize<Location?>(v, MappingOptions.JsonSerializerOptions))
                .HasColumnType("jsonb");

            entity.Property(e => e.AvatarColor)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, MappingOptions.JsonSerializerOptions),
                    v => JsonSerializer.Deserialize<ColorDTO>(v, MappingOptions.JsonSerializerOptions))
                .HasColumnType("jsonb");
        });

        builder.Entity<PlayerLeftGameInfo>(entity =>
        {
            entity.HasOne(l => l.Player);
        });

        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.DefaultAvatarColor)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, MappingOptions.JsonSerializerOptions),
                    v => JsonSerializer.Deserialize<ColorDTO>(v, MappingOptions.JsonSerializerOptions))
                .HasColumnType("jsonb");
        });
        
        base.OnModelCreating(builder);
    }
}
