namespace Waypoint.Api.Middleware;

/// <summary>
/// When maintenance mode is enabled (config "Maintenance:Enabled" = true), every request except the
/// health probe short-circuits with 503 + Retry-After so clients can show the maintenance state and
/// retry (L2-PLATFORM-009).
/// </summary>
public sealed class MaintenanceMiddleware
{
    private const int RetryAfterSeconds = 120;

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public MaintenanceMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var enabled = _configuration.GetValue<bool>("Maintenance:Enabled");
        if (enabled && !context.Request.Path.StartsWithSegments("/health"))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = RetryAfterSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Waypoint is down for scheduled maintenance.",
                detail = "Nothing you've done is lost. Please check again shortly.",
                retryAfterSeconds = RetryAfterSeconds,
            });
            return;
        }

        await _next(context);
    }
}
