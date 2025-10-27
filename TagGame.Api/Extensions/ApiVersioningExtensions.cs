namespace TagGame.Api.Extensions;

public static class ApiVersioningExtensions
{
    public static RouteGroupBuilder MapV1(this IEndpointRouteBuilder app)
    {
        return app.MapGroup("/v1")
            .WithTags("v1")
            .WithGroupName("v1")
            .WithOpenApi();
    }
}
