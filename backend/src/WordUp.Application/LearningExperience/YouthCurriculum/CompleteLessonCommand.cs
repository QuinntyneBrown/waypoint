using MediatR;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>A youth marks a lesson complete. Server-side gating enforces lock/prerequisite/order
/// (L2-LEARN-001 AC3, L2-LEARN-002 AC3) and auto-awards lesson-completed badges.</summary>
public record CompleteLessonCommand(Guid LessonId) : IRequest;
