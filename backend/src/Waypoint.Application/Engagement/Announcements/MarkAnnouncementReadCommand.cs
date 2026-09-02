using MediatR;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>Records that the current user has read an announcement (L2-ENGAGE-006). Idempotent.</summary>
public record MarkAnnouncementReadCommand(Guid AnnouncementId) : IRequest;
