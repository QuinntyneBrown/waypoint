using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.NotificationPreferences;

public class NotificationPreferencesQueryHandler
    : IRequestHandler<NotificationPreferencesQuery, IReadOnlyList<NotificationPreferenceView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NotificationPreferencesQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationPreferenceView>> Handle(
        NotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        return await _db.NotificationPreferences
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.EventKey)
            .Select(p => new NotificationPreferenceView(p.EventKey, p.InApp, p.Email))
            .ToListAsync(cancellationToken);
    }
}
