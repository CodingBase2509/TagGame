using Microsoft.AspNetCore.WebUtilities;

namespace TagGame.Api.Extensions;

public static class ServiceCollectionProblemDetailsExtensions
{
    public static IServiceCollection AddProblemDetailsSupport(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                var status = ctx.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
                ctx.ProblemDetails.Type ??= $"https://httpstatuses.io/{status}";
                ctx.ProblemDetails.Title ??= ReasonPhrases.GetReasonPhrase(status);
                ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;

                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;

                if (env.IsDevelopment() && ctx.Exception is not null)
                {
                    ctx.ProblemDetails.Extensions["exception"] = new
                    {
                        type = ctx.Exception.GetType().FullName,
                        message = ctx.Exception.Message,
                        stackTrace = ctx.Exception.StackTrace,
                        innerException = ctx.Exception.InnerException?.GetType().FullName,
                        innerExceptionMessage = ctx.Exception.InnerException?.Message,
                    };
                }
            };
        });

        return services;
    }
}
