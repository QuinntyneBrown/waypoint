namespace WordUp.Api.Contracts.PlatformServices;

public record UpdateProfileRequest(string DisplayName, string Email, string? Pronouns);
