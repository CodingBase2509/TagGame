using Carter;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using TagGame.Api.Core;
using TagGame.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// DI composition
builder.Services.AddCore(builder.Configuration);
builder.Services.AddSharedJsonOptions();
builder.Services.AddHostHealthChecks(builder.Configuration);
builder.Services.AddOpenApiWithJwt();

builder.Services.AddCarter();
builder.Services.AddDevCors(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddProblemDetailsSupport(builder.Environment);

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

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseCors(ServiceCollectionCorsExtensions.DevCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "TagGame API";
        options.DarkMode = true;
    });
}

app.MapCarter();

app.Run();

public partial class Program {}

public partial class Program;
