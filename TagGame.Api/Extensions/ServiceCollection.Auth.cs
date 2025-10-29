using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Api.Infrastructure.Auth.Handler;

namespace TagGame.Api.Extensions;

public static class ServiceCollectionAuth
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddJwtAuthentication(config);
        services.AddCustomAuthorization();

        return services;
    }

    private static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, AuthPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, RoomMemberHandler>();
        services.AddScoped<IAuthorizationHandler, RoomPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, RoomRoleHandler>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("Jwt");
        var issuer = section["Issuer"] ?? string.Empty;
        var audience = section["Audience"] ?? string.Empty;
        var signingKey = section["SigningKey"] ?? string.Empty;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
