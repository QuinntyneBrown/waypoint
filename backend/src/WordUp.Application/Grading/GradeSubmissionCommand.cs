using MediatR;

namespace WordUp.Application.Grading;

/// <summary>A mentor (or administrator) grades a youth submission (L2-MENTOR-003). Decision is one of
/// "approve", "request-revision", or "save-draft".</summary>
public record GradeSubmissionCommand(
    Guid SubmissionId,
    string Decision,
    string? RubricScoresJson,
    int? TotalAwarded,
    string? Comment,
    bool GuardianNotified) : IRequest;
