using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.DisplayPreferences;

public class DisplayPreferenceQueryHandler : IRequestHandler<DisplayPreferenceQuery, DisplayPreferenceView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DisplayPreferenceQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DisplayPreferenceView> Handle(DisplayPreferenceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        var pref = await _db.DisplayPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        // Defaults when the user has never saved a preference.
        return pref is null
            ? new DisplayPreferenceView(100, false, false, false, null)
            : new DisplayPreferenceView(
                pref.TextScalePercent,
                pref.HighContrast,
                pref.ReduceMotion,
                pref.UnderlineLinks,
                pref.ReadingFont);
    }
}
