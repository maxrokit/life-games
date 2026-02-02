using FluentValidation;
using LifeGames.Application.Behaviors;
using LifeGames.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LifeGames.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<ICycleDetectionService, CycleDetectionService>();

        return services;
    }
}
