using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Auth;
using WordUp.Application.Behaviors;

namespace WordUp.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationDependencyInjection).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<ISessionIssuer, SessionIssuer>();
        return services;
    }
}
