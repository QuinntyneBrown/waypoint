using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;

namespace WordUp.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("WordUp"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
        }

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISecureTokens, SecureTokens>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
