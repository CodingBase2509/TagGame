using Carter;
using FluentValidation;
using TagGame.Api;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("TagGame.Api", new()
    {
        Title = "TagGame API",
        Version = "v1",
    });
});
builder.Services.AddCarter();

builder.Services.AddDbLayer(config);
builder.Services.AddServices();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("swagger.json", "TagGame.Api"));
}

app.UseHttpsRedirection();
app.MapGroup("v1")
    .MapCarter();

app.Run();
