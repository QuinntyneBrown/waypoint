using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

public class ListApplicationsQueryHandler
    : IRequestHandler<ListApplicationsQuery, IReadOnlyList<ApplicationSummary>>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public ListApplicationsQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ApplicationSummary>> Handle(
        ListApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _db.Applications.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
        var youthIds = applications
            .Where(item => item.CreatedYouthUserId != null)
            .Select(item => item.CreatedYouthUserId!.Value)
            .ToList();
        var consentedYouthIds = await _db.GuardianConsents.AsNoTracking()
            .Where(consent => youthIds.Contains(consent.YouthUserId)
                && consent.ParticipationConsent
                && consent.WithdrawnAt == null
                && consent.ValidFrom <= _clock.UtcNow
                && consent.ValidUntil >= _clock.UtcNow)
            .Select(consent => consent.YouthUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var consented = consentedYouthIds.ToHashSet();

        var rows = applications
            .Select(application => new
            {
                Application = application,
                Status = ApplicationReviewState.EffectiveStatus(
                    application,
                    application.CreatedYouthUserId is { } youthId && consented.Contains(youthId)),
            });
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var filter = request.Status.Trim().Replace("_", " ", StringComparison.Ordinal);
            rows = rows.Where(item =>
                string.Equals(item.Status, filter, StringComparison.OrdinalIgnoreCase));
        }

        return rows.Select(row => new ApplicationSummary(
                row.Application.Id,
                row.Application.FirstName,
                row.Application.LastName,
                row.Application.Age,
                row.Application.IsMinor,
                row.Application.Neighbourhood,
                row.Application.TrackInterest,
                row.Status,
                row.Application.CreatedAt))
            .ToList();
    }
}
