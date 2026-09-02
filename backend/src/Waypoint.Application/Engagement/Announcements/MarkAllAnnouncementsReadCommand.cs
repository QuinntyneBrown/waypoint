using MediatR;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>Marks every announcement currently visible to the user as read (L2-ENGAGE-006).</summary>
public record MarkAllAnnouncementsReadCommand() : IRequest;
