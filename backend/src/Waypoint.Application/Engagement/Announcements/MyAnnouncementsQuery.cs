using MediatR;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>The announcements visible to the current user (sent + audience + role targeted), each with a
/// per-user read flag (L2-ENGAGE-006).</summary>
public record MyAnnouncementsQuery() : IRequest<IReadOnlyList<AnnouncementView>>;

public record AnnouncementView(Guid Id, string Title, string Message, DateTime? SentAt, bool Read);
