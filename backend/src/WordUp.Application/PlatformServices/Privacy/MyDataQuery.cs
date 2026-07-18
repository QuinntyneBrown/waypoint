using MediatR;

namespace WordUp.Application.PlatformServices.Privacy;

/// <summary>
/// Returns the caller's own stored personal data for the data-subject "access" right (L2-NFR-003 AC2),
/// scoped strictly to <see cref="Abstractions.ICurrentUser.UserId"/>.
/// </summary>
public record MyDataQuery : IRequest<MyDataResult>;

public record MyDataResult(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    string Status,
    string? Pronouns,
    string? PhotoUrl,
    Guid? CohortId,
    Guid? MentorId,
    Guid? GuardianUserId,
    DateTime CreatedAt,
    DateTime? LastSignInAt);
