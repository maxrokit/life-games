namespace LifeGames.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdLogPropertyName = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header, or generate a new one
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Add to response header so clients can track requests
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Add to log context for structured logging
        using (Serilog.Context.LogContext.PushProperty(CorrelationIdLogPropertyName, correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
