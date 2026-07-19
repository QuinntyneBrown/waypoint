using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;

namespace WordUp.Api.Middleware;

// Traces to: L2-NFR-011 AC2
/// <summary>
/// Emits one structured completion event without reading headers, query values, bodies, or raw URLs.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<RequestLoggingMiddleware> logger)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            logger.LogInformation(
                "HTTP request completed. CorrelationId={CorrelationId} Method={Method} Route={Route} " +
                "StatusCode={StatusCode} ElapsedMilliseconds={ElapsedMilliseconds}",
                context.TraceIdentifier,
                context.Request.Method,
                SafeRequestTelemetry.Route(context),
                context.Response.StatusCode,
                Math.Round(Stopwatch.GetElapsedTime(started).TotalMilliseconds, 2));
        }
    }
}

public static class SafeRequestTelemetry
{
    public static string Route(HttpContext context)
    {
        var selectedEndpoint = context.GetEndpoint()
            ?? context.Features.Get<IExceptionHandlerFeature>()?.Endpoint;
        return selectedEndpoint is RouteEndpoint endpoint
        && !string.IsNullOrWhiteSpace(endpoint.RoutePattern.RawText)
            ? endpoint.RoutePattern.RawText
            : "<unmatched>";
    }
}
