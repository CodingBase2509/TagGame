using Carter;
// using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using TagGame.Api.Core;
using TagGame.Api.Extensions;
using TagGame.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// DI composition
builder.Services.AddCore(builder.Configuration);
builder.Services.AddSharedJsonOptions();
builder.Services.AddHostHealthChecks(builder.Configuration);
builder.Services.AddOpenApiWithJwt();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCarter();
builder.Services.AddDevCors(builder.Configuration, builder.Environment);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddProblemDetailsSupport(builder.Environment);
builder.Services.AddConfiguredSignalR();

var app = builder.Build();

app.UseExceptionHandler();
app.MapStatusCodes();

app.UseAuthentication();
app.UseAuthorization();

app.MapDevOptions();

app.MapCarter();
app.MapHub<LobbyHub>("/hubs/lobby");
app.MapHub<GameHub>("/hubs/game");

app.Run();
