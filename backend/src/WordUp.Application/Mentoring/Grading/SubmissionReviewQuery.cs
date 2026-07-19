using MediatR;

namespace WordUp.Application.Mentoring.Grading;

public sealed record SubmissionReviewQuery(Guid SubmissionId) : IRequest<SubmissionReviewView>;

public sealed record SubmissionReviewView(
    Guid Id,
    Guid YouthId,
    string YouthName,
    string AssignmentTitle,
    string TaskDescription,
    string Status,
    bool IsCodingProject,
    string? BodyText,
    string? PhotoUrl,
    string? LinkUrl,
    SubmissionReviewAttachment? File,
    string? YouthNote,
    DateTime SubmittedAt,
    string? MentorComment,
    int? TotalAwarded,
    int TotalPossible,
    bool GuardianNotified,
    IReadOnlyList<SubmissionReviewCriterion> Rubric);

public sealed record SubmissionReviewAttachment(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);

public sealed record SubmissionReviewCriterion(
    Guid CriterionId,
    string Description,
    int PossiblePoints,
    int? AwardedPoints);
