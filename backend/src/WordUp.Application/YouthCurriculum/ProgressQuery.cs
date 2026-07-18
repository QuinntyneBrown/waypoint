using MediatR;

namespace WordUp.Application.YouthCurriculum;

/// <summary>A youth's aggregate progress dashboard, computed from the single source of truth
/// (LessonProgress / QuizAttempt / CompetencyAssessment) — L2-LEARN-010.</summary>
public record ProgressQuery : IRequest<ProgressView>;

public record ProgressView(
    double OverallPercent,
    double QuizAverage,
    int LessonsCompleted,
    double AttendanceRate,
    IReadOnlyList<CompetencyLevelDto> Competencies);

public record CompetencyLevelDto(Guid CompetencyId, string Level);
