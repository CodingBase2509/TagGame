namespace TagGame.Api.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration) =>
        // Placeholder for Core registration (EF Core, Repositories, Feature Services, Validators, Mapping)
        // Intentionally minimal for v2 skeleton to satisfy DI wiring and build.
        services;
}

