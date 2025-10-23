using Microsoft.EntityFrameworkCore.Design;

namespace TagGame.Api.Core.Persistence.Contexts;

public sealed class DesignTimeAuthFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? "Host=localhost;Port=5432;Database=TagGame;Username=taggame;Password=SecurePassword;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(cs, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory_Auth", "auth");
            })
            .Options;

        return new AuthDbContext(options);
    }
}
