using MediatR;

namespace Waypoint.Application.LearningExperience.QuizTaking;

public sealed record QuizIntroQuery(Guid QuizId) : IRequest<QuizIntroView>;

public sealed record QuizBadgeView(Guid Id, string Name, string Icon, string? Description);

public sealed record QuizIntroView(
    Guid QuizId,
    string Title,
    int QuestionCount,
    int EstimatedMinutes,
    int AttemptsAllowed,
    int PassMark,
    QuizBadgeView? BadgeReward,
    int CurrentAttemptNumber,
    int AttemptsRemaining,
    bool HasInProgressAttempt,
    bool AttemptsExhausted);
