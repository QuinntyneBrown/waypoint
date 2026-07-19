using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Fail fast at startup if JWT configuration is missing/weak (L2-NFR-011 AC4).
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<AuditIntegrityOptions>()
            .Bind(configuration.GetSection("AuditIntegrity"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SensitiveDataOptions>()
            .Bind(configuration.GetSection("SensitiveData"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
        services.AddSingleton<IAuditIntegrity, HmacAuditIntegrity>();
        services.AddSingleton<ISensitiveDataProtector, AesSensitiveDataProtector>();
        services.AddSingleton<IDataRetentionPolicy, ConfigurationDataRetentionPolicy>();
        services.AddSingleton<IAtRiskThresholds, ConfigurationAtRiskThresholds>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
