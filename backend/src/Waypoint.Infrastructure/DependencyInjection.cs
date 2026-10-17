using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

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
        services.AddOptions<ApplicationUrlOptions>()
            .Bind(configuration.GetSection("ApplicationUrls"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection("Smtp"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MediaOptions>()
            .Bind(configuration.GetSection("Media"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IMediaSettings>(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MediaOptions>>().Value);
        services.AddSingleton<FileSystemMediaStore>();
        services.AddSingleton<IMediaStore>(sp => sp.GetRequiredService<FileSystemMediaStore>());
        services.AddSingleton<IUploadStore>(sp => sp.GetRequiredService<FileSystemMediaStore>());
        services.AddSingleton<IVideoTranscoder, FfmpegVideoTranscoder>();
        services.AddScoped<VideoTranscodeProcessor>();
        services.AddHostedService<VideoTranscodeWorker>();

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("Waypoint"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
        }

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserPrivacyLock, SqlServerUserPrivacyLock>();
        services.AddScoped<IVerifiedRecordOwnershipClaimer, VerifiedRecordOwnershipClaimer>();
        services.AddScoped<AfterCommitDispatcher>();
        services.AddScoped<IAfterCommitDispatcher>(sp =>
            sp.GetRequiredService<AfterCommitDispatcher>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AtomicOutboxBehavior<,>));
        services.AddSingleton<IDatabaseErrorClassifier, SqlServerDatabaseErrorClassifier>();
        services.AddScoped<SensitiveDataBackfill>();
        services.AddScoped<IDatabaseLifecycle, DatabaseLifecycle>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISecureTokens, SecureTokens>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddSingleton<IAuditIntegrity, HmacAuditIntegrity>();
        services.AddSingleton<ISensitiveDataProtector, AesSensitiveDataProtector>();
        services.AddSingleton<IDataRetentionPolicy, ConfigurationDataRetentionPolicy>();
        services.AddSingleton<IAtRiskThresholds, ConfigurationAtRiskThresholds>();
        services.AddSingleton<IApplicationLinkBuilder, ApplicationLinkBuilder>();
        services.AddScoped<IEmailSender, OutboxEmailSender>();
        services.AddScoped<OutboundEmailProcessor>();
        services.AddScoped<IOutboundEmailProcessorRunner>(sp =>
            sp.GetRequiredService<OutboundEmailProcessor>());
        services.AddScoped<ScheduledNotificationProcessor>();
        services.AddSingleton<ISmtpEmailTransport, SmtpEmailTransport>();
        services.AddHostedService<OutboundEmailWorker>();
        services.AddHostedService<ScheduledNotificationWorker>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
