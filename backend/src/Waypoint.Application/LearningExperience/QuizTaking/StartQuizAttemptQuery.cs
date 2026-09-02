using MediatR;

namespace Waypoint.Application.LearningExperience.QuizTaking;

/// <summary>Starts (or resumes an unsubmitted) quiz attempt for the current youth (L2-LEARN-004).</summary>
public record StartQuizAttemptQuery(Guid QuizId) : IRequest<QuizAttemptView>;

public record QuizAttemptView(
    Guid AttemptId,
    Guid QuizId,
    string Title,
    int AttemptNumber,
    int QuestionCount,
    int EstimatedMinutes,
    int AttemptsAllowed,
    int PassMark,
    QuizBadgeView? BadgeReward,
    int CurrentQuestionIndex,
    string? AnswersJson,
    IReadOnlyList<QuestionDto> Questions);

/// <summary>A quiz question as shown to the youth. Never carries the correct answer (L2-LEARN-004 AC1).</summary>
public record QuestionDto(
    Guid Id,
    string Text,
    string Type,
    IReadOnlyList<string> Options);
