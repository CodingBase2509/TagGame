using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TagGame.Api.Core;
using TagGame.Api.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DI composition
builder.Services.AddCore(builder.Configuration);
builder.Services.AddHostHealthChecks(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddOpenApiWithJwt();

builder.Services.AddProblemDetailsSupport(builder.Environment);
builder.Services.AddDevCors(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();
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

if (app.Environment.IsDevelopment())
{
    app.UseCors(CorsExtensions.DevCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "TagGame API";
        options.DarkMode = true;
    });
}

app.MapCarter();

app.Run();
