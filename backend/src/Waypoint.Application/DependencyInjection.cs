using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Mentoring.Flags;
using Waypoint.Application.PlatformServices.Privacy;
using Waypoint.Application.Shared.Behaviors;
using Waypoint.Application.Shared.Security;

namespace Waypoint.Application;

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
        services.AddScoped<PersonalDataAnonymizer>();
        services.AddScoped<AuthenticatedPrivacyWriteGuard>();
        return services;
    }
}
