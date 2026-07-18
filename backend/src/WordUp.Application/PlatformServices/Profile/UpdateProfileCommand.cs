using MediatR;

namespace WordUp.Application.PlatformServices.Profile;

/// <summary>Updates the signed-in user's own editable profile fields (L2-PLATFORM-002). Managed
/// placement fields (cohort, mentor, guardian) are read-only and cannot be changed here.</summary>
public record UpdateProfileCommand(
    string DisplayName,
    string Email,
    string? Pronouns,
    string? PhotoUrl) : IRequest;
