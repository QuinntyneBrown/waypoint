using MediatR;

namespace WordUp.Application.Engagement.Rsvps;

/// <summary>Lists the RSVPs for a session so a mentor/admin can see who is coming (and each note).</summary>
public record SessionRsvpsQuery(Guid SessionId) : IRequest<IReadOnlyList<RsvpView>>;

public record RsvpView(Guid YouthUserId, string Response, string? Note, bool ReminderSet);
