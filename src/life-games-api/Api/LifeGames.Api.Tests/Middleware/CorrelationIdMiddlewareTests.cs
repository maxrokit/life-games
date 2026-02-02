using FluentAssertions;
using LifeGames.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace LifeGames.Api.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly DefaultHttpContext _context;

    public CorrelationIdMiddlewareTests()
    {
        _context = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_NoCorrelationIdInRequest_GeneratesNewId()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = _context.Response.Headers["X-Correlation-ID"].ToString();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdInRequest_UsesExistingId()
    {
        var existingId = "test-correlation-id-123";
        _context.Request.Headers.Append("X-Correlation-ID", existingId);

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(_context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MultipleRequests_GeneratesDifferentIds()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();

        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        var id1 = context1.Response.Headers["X-Correlation-ID"].ToString();
        var id2 = context2.Response.Headers["X-Correlation-ID"].ToString();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponse()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
    }
}
