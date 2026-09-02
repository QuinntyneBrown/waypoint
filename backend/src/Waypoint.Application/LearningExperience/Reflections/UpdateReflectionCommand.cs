using MediatR;

namespace Waypoint.Application.LearningExperience.Reflections;

/// <summary>Updates the current youth's own reflection (owner-only); restores or edits a draft
/// (L2-LEARN-003 AC3/AC4).</summary>
public record UpdateReflectionCommand(
    Guid Id,
    string? Text,
    string? Mood,
    IReadOnlyDictionary<string, string>? SecondaryResponses,
    bool ShareWithMentor,
    bool IsDraft) : IRequest;
