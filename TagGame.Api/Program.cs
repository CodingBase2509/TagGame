using TagGame.Api.Core;

var builder = WebApplication.CreateBuilder(args);

// DI composition
builder.Services.AddCore(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
