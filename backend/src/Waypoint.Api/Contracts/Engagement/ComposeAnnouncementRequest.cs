namespace Waypoint.Api.Contracts.Engagement;

public record ComposeAnnouncementRequest(
    string Title,
    string Message,
    string AudienceScope,
    Guid? CohortId,
    string RoleTargeting,
    DateTime? ScheduledAt);
