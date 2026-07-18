using MediatR;

namespace WordUp.Application.Engagement.Rsvps;

public sealed record YouthScheduleQuery : IRequest<IReadOnlyList<YouthSessionView>>;

public sealed record YouthSessionView(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    string? Response,
    string? Note,
    bool ReminderSet,
    DateTime? ReminderAt);
