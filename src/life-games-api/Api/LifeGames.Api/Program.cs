using LifeGames.Api.Extensions;
using LifeGames.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console();

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// Configure services using extension methods
builder.Services.AddApiOptions(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRequestSizeLimits();
builder.Services.AddApiControllers();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecksConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);
bool hasForwardedConfig = false;
builder.Services.AddForwardedHeadersConfiguration(builder.Configuration, builder.Environment, out hasForwardedConfig);
builder.Services.AddResponseCachingConfiguration();
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsProduction() && !hasForwardedConfig)
{
    app.Logger.LogWarning(
        "Forwarded headers are enabled but no known proxies or networks are configured.");
}

// Configure application using extension methods
await app.ApplyDatabaseMigrationsAsync();
app.UseSecurityMiddleware();
app.UseSwaggerDocumentation();
app.UseApiMiddleware();
app.MapApiEndpoints();

app.Run();

public partial class Program { }
