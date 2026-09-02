using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Announcements;

public class MyAnnouncementsQueryHandler : IRequestHandler<MyAnnouncementsQuery, IReadOnlyList<AnnouncementView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public MyAnnouncementsQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<AnnouncementView>> Handle(MyAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        await AnnouncementAccess.ActivateDueAsync(_db, _clock, cancellationToken);
        var query = await AnnouncementAccess.VisibleToAsync(_db, userId, cancellationToken);

        var announcements = await query
            .OrderByDescending(a => a.SentAt)
            .ToListAsync(cancellationToken);

        var readIds = (await _db.AnnouncementReads
                .Where(ar => ar.UserId == userId)
                .Select(ar => ar.AnnouncementId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        return announcements
            .Select(a => new AnnouncementView(a.Id, a.Title, a.Message, a.SentAt, readIds.Contains(a.Id)))
            .ToList();
    }

    // Role targeting values a given role is allowed to see (ENGAGE-006).
    // List<T>.Contains remains a parameterized SQL IN expression on all supported runtimes. Using
    // an array here can bind to the newer ReadOnlySpan overload and fail during EF parameter
    // extraction on preview runtimes.
    internal static List<string> TargetsForRole(string role) => role switch
    {
        Roles.Youth => new List<string> { "Youth", "YouthGuardians", "Everyone" },
        Roles.Guardian => new List<string> { "Guardians", "YouthGuardians", "Everyone" },
        Roles.Mentor => new List<string> { "Mentors", "Everyone" },
        _ => new List<string> { "Everyone" },
    };
}
