namespace WordUp.Api.Contracts.PlatformServices;

public record FulfillDataSubjectRequest(
    string? DisplayName,
    string? Email,
    string? Pronouns,
    string? ResolutionNote);
