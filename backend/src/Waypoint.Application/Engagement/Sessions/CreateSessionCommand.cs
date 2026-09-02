using MediatR;

namespace Waypoint.Application.Engagement.Sessions;

public record CreateSessionCommand(
    Guid CohortId,
    Guid? MentorId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    bool RepeatsWeekly) : IRequest<CreateSessionResult>;

public record CreateSessionResult(Guid FirstSessionId, int GeneratedCount);
