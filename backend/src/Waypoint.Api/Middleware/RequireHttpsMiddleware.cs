namespace Waypoint.Api.Middleware;

/// <summary>Rejects non-TLS production traffic before credentials or tokens can be processed.</summary>
public sealed class RequireHttpsMiddleware
{
    private readonly RequestDelegate _next;

    public RequireHttpsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.IsHttps)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "HTTPS is required.",
        });
    }
}
