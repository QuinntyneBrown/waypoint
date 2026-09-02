using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UserId
    {
        get
        {
            var principal = _accessor.HttpContext?.User;
            var sub = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Role => _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? SessionVersion
    {
        get
        {
            var value = _accessor.HttpContext?.User?.FindFirst("session_version")?.Value;
            return int.TryParse(value, out var version) ? version : null;
        }
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
