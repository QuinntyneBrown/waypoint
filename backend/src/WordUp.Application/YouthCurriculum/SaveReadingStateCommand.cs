using MediatR;

namespace WordUp.Application.YouthCurriculum;

/// <summary>A youth saves their per-lesson reading preferences/position (L2-LEARN-002 AC2).</summary>
public record SaveReadingStateCommand(Guid LessonId, string? TextSize, bool Bookmarked, int Position) : IRequest;
