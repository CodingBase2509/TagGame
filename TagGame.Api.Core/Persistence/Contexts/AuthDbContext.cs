using TagGame.Shared.Domain.Auth;

namespace TagGame.Api.Core.Persistence.Contexts;

/// <summary>
/// Database context for the authentication/identity domain. Contains users,
/// refresh tokens and entitlements/subscriptions. Keep this context separate
/// from gameplay data to preserve bounded contexts while sharing the same DB.
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Application users (can be anonymous and later upgraded).
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Persisted refresh tokens (rotating, one-time use; family tracked).
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// User entitlements/subscriptions (e.g., Premium).
    /// </summary>
    public DbSet<Entitlement> Entitlements { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
