using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

namespace TagGame.Api.Extensions;

public static class WebApplicationEndpoints
{
    public static WebApplication MapStatusCodes(this WebApplication app)
    {
        app.UseStatusCodePages(async context =>
        {
            var svc = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            var status = context.HttpContext.Response.StatusCode;
            if (status >= 400)
            {
                await svc.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails { Status = status },
                });
            }
        });

        return app;
    }

    public static WebApplication MapDevOptions(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.UseCors(ServiceCollectionCorsExtensions.DevCorsPolicy);
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "TagGame API";
            options.DarkMode = true;
        });

        return app;
    }
}
