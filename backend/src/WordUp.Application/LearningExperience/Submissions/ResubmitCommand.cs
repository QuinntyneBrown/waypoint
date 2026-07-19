using MediatR;

namespace WordUp.Application.LearningExperience.Submissions;

/// <summary>A youth resubmits after a mentor requested revisions (L2-LEARN-007 AC3). Owner-only, and
/// only permitted when the submission is in NeedsRevision.</summary>
public record ResubmitCommand(
    Guid Id,
    string? BodyText,
    string? PhotoUrl,
    string? LinkUrl,
    string? ExpectedVersion) : IRequest;
