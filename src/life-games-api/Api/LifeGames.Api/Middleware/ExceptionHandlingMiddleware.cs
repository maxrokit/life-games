using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace LifeGames.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request was cancelled");
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = context.Request.Path
        };

        foreach (var error in exception.Errors)
        {
            if (!problemDetails.Errors.ContainsKey(error.PropertyName))
            {
                problemDetails.Errors[error.PropertyName] = [];
            }

            problemDetails.Errors[error.PropertyName] = [.. problemDetails.Errors[error.PropertyName], error.ErrorMessage];
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        // Never expose exception details to clients in production
        // Exception details are logged but not returned in response
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = context.Request.Path
            // Note: Do not include exception.Message or exception.StackTrace
            // These are logged server-side but not exposed to clients
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
