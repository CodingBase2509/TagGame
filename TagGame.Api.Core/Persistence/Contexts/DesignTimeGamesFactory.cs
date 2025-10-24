using Microsoft.EntityFrameworkCore.Design;

namespace TagGame.Api.Core.Persistence.Contexts;

public sealed class DesignTimeGamesFactory : IDesignTimeDbContextFactory<GamesDbContext>
{
    public GamesDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? "Host=localhost;Port=5432;Database=TagGame;Username=taggame;Password=SecurePassword;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<GamesDbContext>()
            .UseNpgsql(cs, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory_Games", "games");
            })
            .Options;

        return new GamesDbContext(options);
    }
}
