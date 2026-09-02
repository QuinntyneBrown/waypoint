using Microsoft.Extensions.Configuration;

namespace Waypoint.Infrastructure;

public static class InfrastructureStartupValidator
{
    public static void Validate(IConfiguration configuration, string environmentName)
    {
        if (!environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase)) return;

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString)
            || connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Production requires a durable SQL Server connection string at ConnectionStrings:Default.");
        }

        RequireSecret(configuration, "Jwt:SigningKey");
        RequireSecret(configuration, "AuditIntegrity:SigningKey");
        RequireSecret(configuration, "SensitiveData:EncryptionKey");

        RequireHttpsUrl(configuration, "ApplicationUrls:PortalBaseUrl");
        RequireHttpsUrl(configuration, "ApplicationUrls:PublicSiteBaseUrl");

        if (!configuration.GetValue<bool>("Smtp:Enabled"))
            throw new InvalidOperationException("Production requires SMTP delivery at Smtp:Enabled.");
        if (string.IsNullOrWhiteSpace(configuration["Smtp:Host"]))
            throw new InvalidOperationException("Production requires Smtp:Host.");
        if (string.IsNullOrWhiteSpace(configuration["Smtp:FromEmail"]))
            throw new InvalidOperationException("Production requires Smtp:FromEmail.");
        if (!configuration.GetValue<bool>("Smtp:UseSsl")
            && !configuration.GetValue<bool>("Smtp:UseStartTls"))
            throw new InvalidOperationException("Production SMTP must use SSL or STARTTLS.");
        var smtpUser = configuration["Smtp:Username"];
        var smtpPassword = configuration["Smtp:Password"];
        if (string.IsNullOrWhiteSpace(smtpUser) != string.IsNullOrWhiteSpace(smtpPassword))
            throw new InvalidOperationException("Smtp:Username and Smtp:Password must be configured together.");
    }

    private static void RequireHttpsUrl(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException($"Production requires an absolute HTTPS URL at {key}.");
        }
    }

    private static void RequireSecret(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value)
            || value.Length < 32
            || value.StartsWith("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Production requires a non-placeholder secret of at least 32 characters at {key}.");
        }
    }
}
