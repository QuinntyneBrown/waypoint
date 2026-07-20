namespace WordUp.Api.Contracts.PlatformServices;

public sealed record DataSubjectCorrectionRequest(
    string? DisplayName,
    string? Email,
    string? Pronouns,
    bool ClearPronouns = false);

public record DataSubjectActionRequest(
    string ActionType,
    string? Details,
    DataSubjectCorrectionRequest? Correction);
