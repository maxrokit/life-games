using FluentAssertions;
using LifeGames.Api.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;

namespace LifeGames.Api.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly DefaultHttpContext _context;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public SecurityHeadersMiddlewareTests()
    {
        _context = new DefaultHttpContext();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
    }

    [Fact]
    public async Task InvokeAsync_AddsStrictTransportSecurityHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Strict-Transport-Security");
        _context.Response.Headers["Strict-Transport-Security"].ToString()
            .Should().Be("max-age=31536000; includeSubDomains; preload");
    }

    [Fact]
    public async Task InvokeAsync_AddsXContentTypeOptionsHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        _context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_AddsXFrameOptionsHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("X-Frame-Options");
        _context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_AddsXXSSProtectionHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("X-XSS-Protection");
        _context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task InvokeAsync_AddsReferrerPolicyHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Referrer-Policy");
        _context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_AddsPermissionsPolicyHeader()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Permissions-Policy");
        _context.Response.Headers["Permissions-Policy"].ToString()
            .Should().Be("geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), speaker=()");
    }

    [Fact]
    public async Task InvokeAsync_Production_AddsRestrictiveCSP()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        _context.Request.Path = "/api/boards";

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = _context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("script-src 'none'");
        csp.Should().Contain("style-src 'none'");
        csp.Should().Contain("frame-ancestors 'none'");
    }

    [Fact]
    public async Task InvokeAsync_DevelopmentSwagger_AddsRelaxedCSP()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        _context.Request.Path = "/swagger/index.html";

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = _context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("script-src 'self' 'unsafe-inline'");
        csp.Should().Contain("style-src 'self' 'unsafe-inline'");
    }

    [Fact]
    public async Task InvokeAsync_DevelopmentNonSwagger_AddsRestrictiveCSP()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        _context.Request.Path = "/api/boards";

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        _context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = _context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("script-src 'none'");
        csp.Should().Contain("style-src 'none'");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new SecurityHeadersMiddleware(next, _mockEnvironment.Object);

        await middleware.InvokeAsync(_context);

        nextCalled.Should().BeTrue();
    }
}
