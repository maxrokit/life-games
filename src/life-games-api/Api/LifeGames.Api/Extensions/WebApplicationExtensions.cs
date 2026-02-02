using LifeGames.Api.Middleware;
using LifeGames.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LifeGames.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LifeGamesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Check if using a relational provider (in-memory doesn't support migrations)
        if (!db.Database.IsRelational())
        {
            await db.Database.EnsureCreatedAsync();
            return app;
        }

        try
        {
            var hasMigrations = db.Database.GetMigrations().Any();

            if (hasMigrations)
            {
                var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await db.Database.MigrateAsync();
                }
                else
                {
                    logger.LogInformation("Database is up to date. No pending migrations.");
                }
            }
            else if (app.Environment.IsDevelopment())
            {
                await db.Database.EnsureCreatedAsync();
            }
            else
            {
                throw new InvalidOperationException(
                    "No EF Core migrations found. Create and apply migrations for production.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not check or apply migrations at startup. Database might already be configured.");
            // Continue anyway - the database might already be configured via manual migration
        }

        return app;
    }

    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        app.UseExceptionHandling();
        app.UseForwardedHeaders();
        app.UseCorrelationId();
        app.UseSerilogRequestLogging();
        app.UseSecurityHeaders();

        return app;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Life Games API v1");
            });
        }

        return app;
    }

    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseResponseCaching();
        app.UseRateLimiter();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}
