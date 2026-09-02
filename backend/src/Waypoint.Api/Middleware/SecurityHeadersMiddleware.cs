namespace Waypoint.Api.Middleware;

/// <summary>
/// Adds baseline security headers to every response (L2-NFR-002 AC3): Content-Security-Policy
/// (with frame-ancestors 'none'), X-Frame-Options, X-Content-Type-Options, Referrer-Policy, and HSTS.
/// Registered as the first middleware so the headers ride on every response — including the global
/// exception (500) and maintenance (503) short-circuits downstream. Headers are set via OnStarting so
/// they attach even when a downstream component writes the response itself. The Swagger UI (a dev tool
/// serving its own inline scripts) gets a relaxed CSP so it keeps working; API responses are JSON and
/// take the strict policy.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private const string StrictCsp = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
    private const string SwaggerCsp = "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var isSwagger = context.Request.Path.StartsWithSegments("/swagger");

        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Content-Security-Policy"] = isSwagger ? SwaggerCsp : StrictCsp;
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            return Task.CompletedTask;
        });

        return _next(context);
    }
}
