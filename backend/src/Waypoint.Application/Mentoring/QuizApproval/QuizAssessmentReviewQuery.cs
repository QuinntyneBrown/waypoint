using MediatR;

namespace Waypoint.Application.Mentoring.QuizApproval;

public sealed record QuizAssessmentReviewQuery(Guid AttemptId) : IRequest<QuizAssessmentReviewView>;

public sealed record QuizAssessmentReviewView(
    Guid AttemptId,
    Guid YouthId,
    string YouthName,
    Guid QuizId,
    string QuizTitle,
    int AttemptNumber,
    int AutoScore,
    int? ApprovedScore,
    int PassMark,
    bool AutoPassed,
    bool? ApprovedPassed,
    string Status,
    DateTime SubmittedAt,
    DateTime? ApprovedAt,
    string? ApprovedByName,
    IReadOnlyList<QuizAssessmentQuestionView> Questions);

public sealed record QuizAssessmentQuestionView(
    Guid QuestionId,
    string Text,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> SelectedIndexes,
    IReadOnlyList<int> CorrectIndexes,
    bool IsCorrect);
