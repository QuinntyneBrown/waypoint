using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.PublicSite;

public class GetPublicContentQueryHandler : IRequestHandler<GetPublicContentQuery, PublicContentView>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public GetPublicContentQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PublicContentView> Handle(GetPublicContentQuery request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        // Only published, enabled tracks and their published modules are ever exposed publicly (L2-PUB-002).
        var tracks = await _db.Tracks
            .Where(t => t.Status == PublishStatus.Published && t.Enabled)
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.Key, t.Description })
            .ToListAsync(cancellationToken);

        var trackIds = tracks.Select(t => t.Id).ToList();

        var modules = await _db.Modules
            .Where(m => trackIds.Contains(m.TrackId) && m.Status == PublishStatus.Published)
            .OrderBy(m => m.Order)
            .Select(m => new { m.Id, m.TrackId, m.Name, m.Description, m.Order })
            .ToListAsync(cancellationToken);

        var moduleIds = modules.Select(m => m.Id).ToList();

        // Outcome badge for a module = the auto "module-completed" badge targeting it that is not archived.
        var outcomeBadges = await _db.Badges
            .Where(b => b.AutoRule == "module-completed"
                && b.TargetRefId != null
                && moduleIds.Contains(b.TargetRefId.Value)
                && b.ArchivedAt == null)
            .Select(b => new { b.Id, b.Name, b.Icon, b.Description, ModuleId = b.TargetRefId!.Value })
            .ToListAsync(cancellationToken);

        var badgeByModule = outcomeBadges
            .GroupBy(b => b.ModuleId)
            .ToDictionary(g => g.Key, g => g.First());

        var trackViews = tracks
            .Select(t => new PublicTrackView(
                t.Id,
                t.Name,
                t.Key,
                t.Description,
                modules
                    .Where(m => m.TrackId == t.Id)
                    .Select(m => new PublicModuleView(
                        m.Id,
                        m.Name,
                        m.Description,
                        m.Order,
                        badgeByModule.TryGetValue(m.Id, out var badge)
                            ? new PublicBadgeView(badge.Id, badge.Name, badge.Icon, badge.Description)
                            : null))
                    .ToList()))
            .ToList();

        // Cohorts expose ONLY neighbourhood(=Location)/dates/lead-mentor DisplayName/status — never youth,
        // guardian, or member data (L2-PUB-001 AC2).
        var cohorts = await _db.Cohorts
            .OrderBy(c => c.TermStart)
            .Select(c => new
            {
                c.Id,
                c.Name,
                Neighbourhood = c.Location,
                c.TermStart,
                c.TermEnd,
                LeadMentor = _db.Users
                    .Where(u => u.Id == c.MentorId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var cohortViews = cohorts
            .Select(c => new PublicCohortView(
                c.Id,
                c.Name,
                c.Neighbourhood,
                c.TermStart,
                c.TermEnd,
                c.LeadMentor,
                DeriveCohortStatus(c.TermStart, c.TermEnd, now)))
            .ToList();

        var mentorship = new MentorshipModelView(
            "Every youth is paired with a mentor who walks alongside them through the program.",
            new[] { "Weekly check-ins", "Reflection reviews", "Extra support when it's needed" });

        var journey = new List<JourneyStepView>
        {
            new(1, "Apply", "Tell us about yourself and why you want to join."),
            new(2, "Join a cohort", "Get matched to a neighbourhood cohort and a mentor."),
            new(3, "Learn the modules", "Work through your track's modules with weekly support."),
            new(4, "Lead & graduate", "Step into leadership, earn your badges, and graduate."),
        };

        // Impact stats are derived from safe structural counts only — no youth/member counts (L2-PUB-001 AC2).
        var impact = new ImpactStatsView(
            Tracks: trackViews.Count,
            Cohorts: cohortViews.Count,
            Modules: modules.Count,
            Neighbourhoods: cohorts
                .Where(c => !string.IsNullOrWhiteSpace(c.Neighbourhood))
                .Select(c => c.Neighbourhood!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());

        return new PublicContentView(trackViews, cohortViews, mentorship, journey, impact);
    }

    /// <summary>
    /// Single authoritative derivation of a cohort's public status from its term window vs the clock,
    /// so every public page reports the same status for the same cohort (L2-PUB-002 AC4).
    /// </summary>
    private static string DeriveCohortStatus(DateTime termStart, DateTime termEnd, DateTime now)
    {
        if (termStart > now)
        {
            return "Upcoming";
        }

        if (termEnd < now)
        {
            return "Completed";
        }

        return "Active";
    }
}
