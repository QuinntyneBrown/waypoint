using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.DisplayPreferences;

public class SaveDisplayPreferenceCommandHandler
    : IRequestHandler<SaveDisplayPreferenceCommand, DisplayPreferenceView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SaveDisplayPreferenceCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DisplayPreferenceView> Handle(
        SaveDisplayPreferenceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        var pref = await _db.DisplayPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (pref is null)
        {
            pref = new DisplayPreference { UserId = userId };
            _db.DisplayPreferences.Add(pref);
        }

        pref.TextScalePercent = request.TextScalePercent;
        pref.HighContrast = request.HighContrast;
        pref.ReduceMotion = request.ReduceMotion;
        pref.UnderlineLinks = request.UnderlineLinks;
        pref.ReadingFont = request.ReadingFont;
        pref.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new DisplayPreferenceView(
            pref.TextScalePercent,
            pref.HighContrast,
            pref.ReduceMotion,
            pref.UnderlineLinks,
            pref.ReadingFont);
    }
}
