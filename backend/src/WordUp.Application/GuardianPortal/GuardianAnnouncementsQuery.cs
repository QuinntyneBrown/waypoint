using MediatR;

namespace WordUp.Application.GuardianPortal;

/// <summary>Announcements visible to a guardian: org-wide plus their child's cohort, targeted to
/// Guardians/YouthGuardians/Everyone, sent only, each with a per-user read flag (L2-GUARDIAN-002).</summary>
public record GuardianAnnouncementsQuery() : IRequest<IReadOnlyList<GuardianAnnouncementView>>;

public record GuardianAnnouncementView(Guid Id, string Title, string Message, DateTime? SentAt, bool Read);
