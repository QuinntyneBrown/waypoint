using MediatR;

namespace WordUp.Application.Engagement.Announcements;

public record ManageAnnouncementsQuery() : IRequest<IReadOnlyList<ManagedAnnouncement>>;

public record ManagedAnnouncement(
    Guid Id,
    string Title,
    string Message,
    string AudienceScope,
    Guid? CohortId,
    string? CohortName,
    string RoleTargeting,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    DateTime CreatedAt,
    int Recipients,
    int Reads,
    double ReadRate);
