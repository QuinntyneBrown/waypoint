using Microsoft.AspNetCore.Hosting;

namespace Waypoint.Api.Security;

internal sealed class SensitiveRateLimitSettings
{
    public int PermitLimit { get; set; } = 20;
    public int WindowSeconds { get; set; } = 60;
}

internal static class SecurityBoundaryConfiguration
{
    public static string[] GetAllowedCorsOrigins(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var configured = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();
        var origins = configured
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => ValidateOrigin(origin, environment.IsProduction()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return origins;
    }

    public static SensitiveRateLimitSettings GetSensitiveRateLimit(IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("Security:SensitiveRateLimit")
            .Get<SensitiveRateLimitSettings>() ?? new SensitiveRateLimitSettings();
        if (settings.PermitLimit is < 1 or > 10_000)
        {
            throw new InvalidOperationException(
                "Security:SensitiveRateLimit:PermitLimit must be between 1 and 10000.");
        }
        if (settings.WindowSeconds is < 1 or > 86_400)
        {
            throw new InvalidOperationException(
                "Security:SensitiveRateLimit:WindowSeconds must be between 1 and 86400.");
        }

        return settings;
    }

    public static bool IsSensitiveEndpoint(PathString path) =>
        path.StartsWithSegments("/api/auth")
        || path.StartsWithSegments("/api/invitations")
        || (path.StartsWithSegments("/api/people")
            && path.Value?.EndsWith("/send-password-reset", StringComparison.OrdinalIgnoreCase) == true);

    private static string ValidateOrigin(string configuredOrigin, bool requireHttps)
    {
        var value = configuredOrigin.Trim().TrimEnd('/');
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || uri.AbsolutePath != "/"
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException(
                $"Cors:AllowedOrigins contains invalid origin '{configuredOrigin}'.");
        }
        if (requireHttps && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"Production CORS origin '{configuredOrigin}' must use HTTPS.");
        }

        return uri.GetLeftPart(UriPartial.Authority);
    }
}
