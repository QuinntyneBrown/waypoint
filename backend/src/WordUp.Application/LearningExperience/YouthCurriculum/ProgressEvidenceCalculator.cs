using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>Role-neutral progress evidence derived from approved outcomes and mentor assessments.</summary>
public static class ProgressEvidenceCalculator
{
    public static async Task<List<QuizHistoryItem>> QuizHistoryAsync(
        IAppDbContext db,
        Guid youthId,
        CancellationToken cancellationToken)
    {
        var attempts = await db.QuizAttempts.AsNoTracking()
            .Where(item => item.YouthUserId == youthId
                && item.IsSubmitted
                && item.ApprovedScore != null
                && item.ApprovedAt != null)
            .OrderByDescending(item => item.ApprovedAt)
            .ToListAsync(cancellationToken);
        var quizIds = attempts.Select(item => item.QuizId).Distinct().ToList();
        var quizzes = await db.Quizzes.AsNoTracking()
            .Where(item => quizIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        var moduleIds = quizzes
            .Where(item => item.ModuleId != null)
            .Select(item => item.ModuleId!.Value)
            .Distinct()
            .ToList();
        var lessonIds = quizzes
            .Where(item => item.LessonId != null)
            .Select(item => item.LessonId!.Value)
            .Distinct()
            .ToList();
        var lessons = await db.Lessons.AsNoTracking()
            .Where(item => lessonIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        moduleIds.AddRange(lessons.Values.Select(item => item.ModuleId));
        moduleIds = moduleIds.Distinct().ToList();
        var modules = await db.Modules.AsNoTracking()
            .Where(item => moduleIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var result = new List<QuizHistoryItem>();
        foreach (var attempt in attempts)
        {
            var quiz = quizzes.FirstOrDefault(item => item.Id == attempt.QuizId);
            if (quiz is null) continue;
            var lesson = quiz.LessonId is { } lessonId ? lessons.GetValueOrDefault(lessonId) : null;
            var moduleId = quiz.ModuleId ?? lesson?.ModuleId;
            var moduleName = moduleId is { } resolvedModuleId
                ? modules.GetValueOrDefault(resolvedModuleId, "Learning module")
                : "Learning module";
            var quizTitle = lesson is null ? $"{moduleName} quiz" : $"{lesson.Title} quiz";
            var score = attempt.ApprovedScore!.Value;
            result.Add(new QuizHistoryItem(
                attempt.Id,
                quiz.Id,
                quizTitle,
                moduleName,
                score,
                score >= quiz.PassMark,
                attempt.ApprovedAt!.Value));
        }
        return result;
    }

    public static async Task<List<CompetencyLevelDto>> CompetenciesAsync(
        IAppDbContext db,
        Guid youthId,
        CancellationToken cancellationToken)
    {
        var assessments = await db.CompetencyAssessments.AsNoTracking()
            .Where(item => item.YouthUserId == youthId)
            .ToListAsync(cancellationToken);
        var latest = assessments
            .GroupBy(item => item.CompetencyId)
            .Select(group => group.OrderByDescending(item => item.AssessedAt).First())
            .ToList();
        var ids = latest.Select(item => item.CompetencyId).ToList();
        var definitions = await db.Competencies.AsNoTracking()
            .Where(item => ids.Contains(item.Id) && item.ArchivedAt == null)
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return latest
            .Where(item => definitions.ContainsKey(item.CompetencyId))
            .Select(item => new CompetencyLevelDto(
                item.CompetencyId,
                definitions[item.CompetencyId],
                item.Level.ToString(),
                PercentFor(item.Level),
                item.AssessedAt))
            .OrderBy(item => item.Name)
            .ToList();
    }

    private static int PercentFor(CompetencyLevel level) => level switch
    {
        CompetencyLevel.Developing => 40,
        CompetencyLevel.Proficient => 75,
        CompetencyLevel.Mastered => 100,
        _ => 0,
    };
}
