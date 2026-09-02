using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.PlatformServices.NotificationPreferences;

public class SaveNotificationPreferenceCommandHandler : IRequestHandler<SaveNotificationPreferenceCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SaveNotificationPreferenceCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        var existing = await _db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.EventKey == request.EventKey, cancellationToken);

        if (existing is null)
        {
            _db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = userId,
                EventKey = request.EventKey,
                InApp = request.InApp,
                Email = request.Email,
            });
        }
        else
        {
            existing.InApp = request.InApp;
            existing.Email = request.Email;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
