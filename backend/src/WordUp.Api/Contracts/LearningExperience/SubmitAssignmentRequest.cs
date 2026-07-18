namespace WordUp.Api.Contracts.LearningExperience;

public record SubmitAssignmentRequest(
    string? BodyText,
    Guid? PhotoUploadId,
    string? PhotoUrl,
    string? LinkUrl,
    bool IsCodingProject,
    string? YouthNote,
    bool Attest);
