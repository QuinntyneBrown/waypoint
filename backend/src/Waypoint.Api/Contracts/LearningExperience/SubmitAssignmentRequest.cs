namespace Waypoint.Api.Contracts.LearningExperience;

public record SubmitAssignmentRequest(
    string? BodyText,
    Guid? PhotoUploadId,
    Guid? FileUploadId,
    string? LinkUrl,
    bool IsCodingProject,
    string? YouthNote,
    bool Attest);
