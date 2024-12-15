using System.Reflection;
using Carter;
using FluentValidation;
using TagGame.Api;
using TagGame.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

#region Service Registration
// Endpoint configuration
builder.Services.AddSwagger();
builder.Services.AddCarter();
builder.Services.AddAuthorization();
builder.Services.AddMiddleware();

// Register services
builder.Services.AddDbLayer(config);
builder.Services.AddServices();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
#endregion

#region Request Pipeline
var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TagGame API");
    });
    
}

app.UseAuthorization();
app.MapCarter();
#endregion

app.Run();

/// <summary>
/// The partial class is for accessibility for the integration tests. 
/// </summary>
public partial class Program { }