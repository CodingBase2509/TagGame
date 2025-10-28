using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace TagGame.Api.Extensions;

/// <summary>
/// ServiceCollection extensions for OpenAPI/Swagger with JWT bearer security.
/// </summary>
public static class ServiceCollectionOpenApiExtensions
{
    /// <summary>
    /// Adds OpenAPI generation and configures a JWT bearer security scheme for the document.
    /// </summary>
    public static IServiceCollection AddOpenApiWithJwt(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "TagGame API",
                    Version = "v1",
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
                };

                document.SecurityRequirements ??= [];
                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = Array.Empty<string>()
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
