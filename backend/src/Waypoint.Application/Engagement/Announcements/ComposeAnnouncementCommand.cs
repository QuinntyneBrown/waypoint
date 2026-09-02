using MediatR;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>Staff compose an announcement (L2-ENGAGE-006). Sent immediately when <see cref="ScheduledAt"/>
/// is null or in the past; otherwise it stays queued (SentAt null) until its scheduled time.</summary>
public record ComposeAnnouncementCommand(
    string Title,
    string Message,
    string AudienceScope,
    Guid? CohortId,
    string RoleTargeting,
    DateTime? ScheduledAt) : IRequest<AnnouncementResult>;

public record AnnouncementResult(Guid Id, DateTime? SentAt);
