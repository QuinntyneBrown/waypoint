using MediatR;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>Delivery stats for an announcement: how many recipients it targeted, how many read it, and
/// the read rate (L2-ENGAGE-006).</summary>
public record AnnouncementStatsQuery(Guid AnnouncementId) : IRequest<AnnouncementStats>;

public record AnnouncementStats(int Recipients, int Reads, double ReadRate);
