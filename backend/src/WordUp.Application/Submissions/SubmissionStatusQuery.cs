using MediatR;

namespace WordUp.Application.Submissions;

/// <summary>A youth checks the status of one of their own submissions (L2-LEARN-007 AC1/AC2/AC3).
/// Owner-only.</summary>
public record SubmissionStatusQuery(Guid Id) : IRequest<SubmissionStatusView>;

public record SubmissionStatusView(
    Guid Id,
    string Status,
    string? MentorComment,
    int? TotalAwarded,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);
