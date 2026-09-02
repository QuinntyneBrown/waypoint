using MediatR;

namespace Waypoint.Application.PublicSite;

/// <summary>
/// Anonymous-safe read of the marketing content for the public site: published tracks with their
/// published modules + outcome badges, the current cohorts with a single authoritative status, the
/// mentorship model, the 4-step journey, and impact statistics derived from safe structural counts.
/// No youth/guardian/mentor PII, emails, member counts, or raw GUIDs beyond ids are exposed
/// (L2-PUB-001 AC2, L2-PUB-002).
/// </summary>
public record GetPublicContentQuery() : IRequest<PublicContentView>;

public record PublicContentView(
    IReadOnlyList<PublicTrackView> Tracks,
    IReadOnlyList<PublicCohortView> Cohorts,
    MentorshipModelView Mentorship,
    IReadOnlyList<JourneyStepView> Journey,
    ImpactStatsView Impact);

public record PublicTrackView(
    Guid Id,
    string Name,
    string Key,
    string? Description,
    IReadOnlyList<PublicModuleView> Modules);

public record PublicModuleView(
    Guid Id,
    string Name,
    string? Description,
    int Order,
    PublicBadgeView? OutcomeBadge);

public record PublicBadgeView(Guid Id, string Name, string Icon, string? Description);

/// <summary>A cohort as shown publicly — only neighbourhood, dates, lead mentor name, and derived status.</summary>
public record PublicCohortView(
    Guid Id,
    string Name,
    string? Neighbourhood,
    DateTime TermStart,
    DateTime TermEnd,
    string? LeadMentor,
    string Status);

public record MentorshipModelView(string Summary, IReadOnlyList<string> Features);

public record JourneyStepView(int Step, string Title, string Description);

public record ImpactStatsView(int Tracks, int Cohorts, int Modules, int Neighbourhoods);
