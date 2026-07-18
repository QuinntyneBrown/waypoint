using MediatR;

namespace WordUp.Application.LearningExperience.Submissions;

/// <summary>A youth submits work for an assignment (L2-LEARN-006; coding projects use the same command
/// with <see cref="IsCodingProject"/> = true, L2-LEARN-008 AC2). Operates on the current youth.</summary>
public record SubmitAssignmentCommand(
    Guid AssignmentId,
    string? BodyText,
    Guid? PhotoUploadId,
    string? PhotoUrl,
    string? LinkUrl,
    bool IsCodingProject,
    string? YouthNote,
    bool Attest) : IRequest<SubmissionResult>;
