using MediatR;

namespace WordUp.Application.Volunteer;

/// <summary>The youth roster for a volunteer's assigned session: each youth's name plus their current
/// Present/Absent mark only (L2-VOLUNTEER-001 AC2). Exposes no progress, at-risk flags, notes or other
/// safeguarding data.</summary>
public record VolunteerRosterQuery(Guid SessionId) : IRequest<IReadOnlyList<VolunteerRosterEntry>>;

public record VolunteerRosterEntry(Guid YouthUserId, string Name, string? Mark);
