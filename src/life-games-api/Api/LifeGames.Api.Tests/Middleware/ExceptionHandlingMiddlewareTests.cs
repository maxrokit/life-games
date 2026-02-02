using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using LifeGames.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LifeGames.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private readonly DefaultHttpContext _context;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        nextCalled.Should().BeTrue();
        _context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400WithProblemDetails()
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("Cells", "Cells collection cannot be null"),
            new("Name", "Board name cannot exceed 200 characters")
        };
        var validationException = new ValidationException(validationFailures);

        RequestDelegate next = _ => throw validationException;
        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.StatusCode.Should().Be(400);
        _context.Response.ContentType.Should().Be("application/problem+json");

        _context.Response.Body.Position = 0;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(_context.Response.Body, options);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("One or more validation errors occurred");
        problemDetails.Errors.Should().ContainKey("Cells");
        problemDetails.Errors.Should().ContainKey("Name");
        problemDetails.Errors["Cells"].Should().Contain("Cells collection cannot be null");
        problemDetails.Errors["Name"].Should().Contain("Board name cannot exceed 200 characters");
    }

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_Returns499()
    {
        RequestDelegate next = _ => throw new OperationCanceledException();
        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.StatusCode.Should().Be(499);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500WithProblemDetails()
    {
        var exception = new InvalidOperationException("Test exception");
        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.StatusCode.Should().Be(500);
        _context.Response.ContentType.Should().Be("application/problem+json");

        _context.Response.Body.Position = 0;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(_context.Response.Body, options);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("An error occurred while processing your request");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_DoesNotExposeExceptionDetails()
    {
        var exception = new InvalidOperationException("Sensitive internal error message");
        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Body.Position = 0;
        var body = await new StreamReader(_context.Response.Body).ReadToEndAsync();

        body.Should().NotContain("Sensitive internal error message");
        body.Should().NotContain("StackTrace");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_IncludesInstancePath()
    {
        _context.Request.Path = "/api/boards";
        var validationFailures = new List<ValidationFailure>
        {
            new("Cells", "Cells collection cannot be null")
        };
        var validationException = new ValidationException(validationFailures);

        RequestDelegate next = _ => throw validationException;
        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Body.Position = 0;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(_context.Response.Body, options);

        problemDetails.Should().NotBeNull();
        problemDetails!.Instance.Should().Be("/api/boards");
    }
}
