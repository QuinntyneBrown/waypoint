namespace WordUp.Api.Contracts.LearningExperience;

public record SubmitAssignmentRequest(
    string? BodyText,
    string? PhotoUrl,
    string? LinkUrl,
    bool IsCodingProject,
    bool Attest);
