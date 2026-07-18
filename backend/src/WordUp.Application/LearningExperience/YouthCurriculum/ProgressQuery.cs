using MediatR;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>A youth's aggregate progress dashboard, computed from the single source of truth
/// (LessonProgress / QuizAttempt / CompetencyAssessment) — L2-LEARN-010.</summary>
public record ProgressQuery : IRequest<ProgressView>;

public record ProgressView(
    double OverallPercent,
    double QuizAverage,
    int LessonsCompleted,
    int LessonsTotal,
    double AttendanceRate,
    IReadOnlyList<QuizHistoryItem> QuizHistory,
    IReadOnlyList<AttendanceHistoryItem> AttendanceHistory,
    IReadOnlyList<CompetencyLevelDto> Competencies);

public record QuizHistoryItem(
    Guid AttemptId,
    Guid QuizId,
    string QuizTitle,
    string ModuleName,
    int Score,
    bool Passed,
    DateTime ApprovedAt);

public record AttendanceHistoryItem(
    Guid SessionId,
    string SessionTitle,
    DateTime StartsAt,
    string Status);

public record CompetencyLevelDto(
    Guid CompetencyId,
    string Name,
    string Level,
    int Percent,
    DateTime AssessedAt);
