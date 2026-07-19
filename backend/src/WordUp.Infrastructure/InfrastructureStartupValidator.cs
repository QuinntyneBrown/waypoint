using Microsoft.Extensions.Configuration;

namespace WordUp.Infrastructure;

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
