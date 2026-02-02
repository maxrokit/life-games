namespace LifeGames.Api.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // HSTS - Force HTTPS for 1 year, include subdomains, allow preloading
        context.Response.Headers.Append("Strict-Transport-Security", 
            "max-age=31536000; includeSubDomains; preload");

        // Prevent MIME-sniffing attacks
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Enable browser XSS protection
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Control referrer information leakage
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy - Adjust for Swagger in Development
        if (environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/swagger"))
        {
            // Relaxed CSP for Swagger UI
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self';");
        }
        else
        {
            // Restrictive CSP for API endpoints
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; script-src 'none'; style-src 'none'; img-src 'self' data:; font-src 'none'; connect-src 'self'; frame-ancestors 'none';");
        }

        // Permissions Policy - Restrict browser features
        context.Response.Headers.Append("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), speaker=()");

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
