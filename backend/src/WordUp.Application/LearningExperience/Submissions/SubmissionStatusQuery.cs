using MediatR;

namespace WordUp.Application.LearningExperience.Submissions;

/// <summary>A youth checks the status of one of their own submissions (L2-LEARN-007 AC1/AC2/AC3).
/// Owner-only.</summary>
public record SubmissionStatusQuery(Guid Id) : IRequest<SubmissionStatusView>;

public record SubmissionStatusView(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    string Status,
    bool IsCodingProject,
    string? BodyText,
    string? YouthNote,
    string? PhotoUrl,
    string? LinkUrl,
    string? MentorComment,
    int? TotalAwarded,
    int TotalPossible,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    IReadOnlyList<SubmissionTimelineItem> Timeline,
    IReadOnlyList<SubmissionRubricScore> Rubric,
    SubmissionBadgeView? EarnedBadge);

public record SubmissionTimelineItem(string Type, string Label, DateTime? At, bool Current);
public record SubmissionRubricScore(
    Guid CriterionId,
    string Description,
    int PossiblePoints,
    int? AwardedPoints);
public record SubmissionBadgeView(Guid Id, string Name, string Icon, string? Description, DateTime AwardedAt);
