using Asp.Versioning;
using LifeGames.Api.Options;
using LifeGames.Application;
using LifeGames.Application.Options;
using LifeGames.Infrastructure;
using LifeGames.Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace LifeGames.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BoardOptions>(
            configuration.GetSection(BoardOptions.SectionName));
        services.Configure<RateLimitingOptions>(
            configuration.GetSection(RateLimitingOptions.SectionName));

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(
            configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(LocalDB)\\MSSQLLocalDB;Database=LifeGames;Integrated Security=true;Encrypt=false");

        return services;
    }

    public static IServiceCollection AddRequestSizeLimits(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
        });
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
        });

        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Support vendor-specific media types for API versioning
            options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/vnd.lifegames.v1+json");
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Ensure vendor media types are supported in content negotiation
            options.SuppressMapClientErrors = false;
        });

        return services;
    }

    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            // Use only media type versioning: Accept: application/vnd.lifegames.v1+json
            options.ApiVersionReader = new MediaTypeApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Life Games API",
                Version = "v1",
                Description = "Conway's Game of Life REST API - Implements Conway's Game of Life with cycle detection and HATEOAS links"
            });

            // Enable XML comments for better documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Document the vendor media types
            options.MapType<object>(() => new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true
            });
        });

        return services;
    }

    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<LifeGamesDbContext>();

        return services;
    }

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                    return;
                }

                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddForwardedHeadersConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        out bool hasForwardedConfig)
    {
        var forwardedProxies = configuration
            .GetSection("ForwardedHeaders:KnownProxies")
            .Get<string[]>() ?? [];
        var forwardedNetworks = configuration
            .GetSection("ForwardedHeaders:KnownNetworks")
            .Get<string[]>() ?? [];
        hasForwardedConfig = forwardedProxies.Length > 0 || forwardedNetworks.Length > 0;

        var hasFwdConfig = hasForwardedConfig;
        var isDev = environment.IsDevelopment();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            if (isDev)
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                return;
            }

            if (!hasFwdConfig)
            {
                return;
            }

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var proxy in forwardedProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (var network in forwardedNetworks)
            {
                options.KnownNetworks.Add(Microsoft.AspNetCore.HttpOverrides.IPNetwork.Parse(network));
            }
        });

        return services;
    }

    public static IServiceCollection AddResponseCachingConfiguration(this IServiceCollection services)
    {
        services.AddResponseCaching();
        return services;
    }

    public static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            var rateLimitingConfig = configuration
                .GetSection(RateLimitingOptions.SectionName)
                .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

            options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
                context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingConfig.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingConfig.WindowSeconds)
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
