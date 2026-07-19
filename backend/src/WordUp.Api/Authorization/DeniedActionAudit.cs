using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.Authorization;

internal static class DeniedActionAudit
{
    public static async Task RecordAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var subject = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(subject, out var actorUserId))
        {
            return;
        }

        await using var scope = context.RequestServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateAsyncScope();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
        await audit.LogAsync(
            "ProtectedActionDenied",
            actorUserId,
            "Endpoint",
            details: $"{context.Request.Method} {context.Request.Path.Value ?? "/"}",
            cancellationToken: CancellationToken.None);
    }
}
