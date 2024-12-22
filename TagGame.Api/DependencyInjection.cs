using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TagGame.Api.Middleware;
using TagGame.Api.Persistence;
using TagGame.Api.Services;

namespace TagGame.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "TagGame API",
                Version = "v1",
                Description = "Api for the TagGame App"
            });
    
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            
            c.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                In = ParameterLocation.Header,
                Description = "Geben Sie 'Basic <Base64-codierte Benutzer-ID>' ein."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "BasicAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }
    
    public static IServiceCollection AddDbLayer(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<GamesDbContext>(options => 
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDataAccess, DataService>();
        
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<GameRoomService>();
        services.AddScoped<PlayerService>();
        services.AddScoped<UserService>();
        
        return services;
    }

    public static IServiceCollection AddMiddleware(this IServiceCollection services)
    {
        //services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
        services.AddSingleton<ExceptionHandlerMiddleware>();
        return services;
    }
}
