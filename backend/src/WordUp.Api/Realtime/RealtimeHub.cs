using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.Realtime;

/// <summary>
/// Authenticated receive-only real-time hub (L2-NFR-005). The token is validated by the standard JWT
/// bearer handler (see Program.cs; websocket clients pass it as ?access_token). A connection records
/// its authenticated session snapshot, but <see cref="SignalRChangePublisher"/> authoritatively
/// revalidates current account and targeting state before every outbound event.
/// </summary>
[Authorize]
public sealed class RealtimeHub : Hub
{
    private readonly IAppDbContext _db;
    private readonly RealtimeConnectionRegistry _connections;
    private readonly IClock _clock;

    public RealtimeHub(
        IAppDbContext db,
        RealtimeConnectionRegistry connections,
        IClock clock)
    {
        _db = db;
        _connections = connections;
        _clock = clock;
    }

    public override async Task OnConnectedAsync()
    {
        var sub = Context.UserIdentifier
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var sessionVersion = Context.User?.FindFirst("session_version")?.Value;
        var expires = Context.User?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
            ?? Context.User?.FindFirst("exp")?.Value;

        if (!Guid.TryParse(sub, out var userId)
            || string.IsNullOrWhiteSpace(role)
            || !int.TryParse(sessionVersion, out var tokenSessionVersion)
            || !long.TryParse(
                expires,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var expiresAtUnixSeconds))
        {
            Context.Abort();
            return;
        }
        var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnixSeconds).UtcDateTime;
        if (expiresAtUtc <= _clock.UtcNow)
        {
            Context.Abort();
            return;
        }

        var user = await _db.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Role,
                item.Status,
                item.SessionVersion,
                item.PersonalDataAnonymizedAt,
            })
            .FirstOrDefaultAsync(Context.ConnectionAborted);
        if (user is null
            || user.Status != UserStatus.Active
            || user.PersonalDataAnonymizedAt is not null
            || user.SessionVersion != tokenSessionVersion
            || !string.Equals(user.Role, role, StringComparison.Ordinal))
        {
            Context.Abort();
            return;
        }

        _connections.Register(
            Context.ConnectionId,
            userId,
            tokenSessionVersion,
            role,
            expiresAtUtc);
        try
        {
            await base.OnConnectedAsync();
        }
        catch
        {
            _connections.Remove(Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
