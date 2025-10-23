using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Core.Persistence.Contexts;

/// <summary>
/// Database context for gameplay/lobby domain: rooms, memberships, matches and rounds.
/// Keep gameplay separate from authentication data.
/// </summary>
public class GamesDbContext(DbContextOptions<GamesDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Game rooms (aggregate root).
    /// </summary>
    public DbSet<GameRoom> GameRooms { get; set; }

    /// <summary>
    /// Memberships of users in rooms including role and permissions.
    /// </summary>
    public DbSet<RoomMembership> Memberships { get; set; }

    /// <summary>
    /// Matches hosted in rooms.
    /// </summary>
    public DbSet<Match> Matches { get; set; }

    /// <summary>
    /// Rounds within a match.
    /// </summary>
    public DbSet<Round> Rounds { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("games");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
