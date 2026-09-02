namespace Waypoint.Application.LearningExperience.QuizTaking;

/// <summary>The result a youth may see. Score/Passed stay null until a mentor approves the attempt
/// (L2-LEARN-005 AC3, L2-MENTOR-004).</summary>
public sealed record QuizQuestionReview(
    Guid QuestionId,
    string Text,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> SelectedIndexes,
    IReadOnlyList<int> CorrectIndexes,
    bool Correct);

public sealed record QuizResultView(
    Guid AttemptId,
    Guid QuizId,
    int AttemptNumber,
    int? Score,
    int PassMark,
    bool? Passed,
    string Status,
    string FeedbackMode,
    IReadOnlyList<QuizQuestionReview> Review,
    QuizBadgeView? EarnedBadge,
    int AttemptsRemaining,
    bool CanRetake,
    int? BestPassingScore);
