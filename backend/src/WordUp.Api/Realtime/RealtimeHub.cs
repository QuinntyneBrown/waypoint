using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.Realtime;

/// <summary>
/// Authenticated real-time delivery hub (L2-NFR-005). On connect, a client is placed only into the
/// groups its authenticated identity authorizes — its own user group, its role group, and (for users
/// with a cohort) its cohort group — so it can never receive events targeted at others. Clients only
/// receive; the server pushes via <see cref="IChangePublisher"/>. The token is validated by the
/// standard JWT bearer handler (see Program.cs; websocket clients pass it as ?access_token).
/// </summary>
[Authorize]
public sealed class RealtimeHub : Hub
{
    private readonly IAppDbContext _db;

    public RealtimeHub(IAppDbContext db) => _db = db;

    public override async Task OnConnectedAsync()
    {
        var sub = Context.UserIdentifier
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        if (Guid.TryParse(sub, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.User(userId));

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Role(user.Role));
                if (user.CohortId is Guid cohortId)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Cohort(cohortId));
                }
            }
        }

        await base.OnConnectedAsync();
    }
}
