using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Mentoring.Flags;
using WordUp.Application.Shared.Behaviors;

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
        services.AddScoped<AtRiskEvaluator>();
        services.AddScoped<NotificationDeliveryService>();
        return services;
    }
}
