using Carter;
using TagGame.Api.Core;
using TagGame.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// DI composition
builder.Services.AddCore(builder.Configuration);
builder.Services.AddHostHealthChecks(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCarter();

app.Run();
