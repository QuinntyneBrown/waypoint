using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

/// <summary>
/// Single source of truth for per-module status + gating for a youth (L2-LEARN-001/002/009/010).
/// A module is complete for a youth when all of its published lessons have a Completed LessonProgress.
/// A module is locked when its prerequisite module is not yet complete for that youth.
/// </summary>
internal static class CurriculumProgressCalculator
{
    public static async Task<AccessibleLesson> EnsureLessonAccessibleAsync(
        IAppDbContext db,
        Guid lessonId,
        Guid youthId,
        CancellationToken ct)
    {
        var lesson = await db.Lessons.FirstOrDefaultAsync(
            item => item.Id == lessonId &&
                    item.Status == PublishStatus.Published &&
                    item.ArchivedAt == null,
            ct) ?? throw new NotFoundException("Lesson not found.");
        var module = await db.Modules.FirstOrDefaultAsync(item => item.Id == lesson.ModuleId, ct)
            ?? throw new NotFoundException("Module not found.");
        var enrolledTrackIds = await ResolveYouthTrackIdsAsync(db, youthId, ct);
        if (module.Status != PublishStatus.Published || !enrolledTrackIds.Contains(module.TrackId))
        {
            throw new ForbiddenException("This lesson is locked.");
        }

        var moduleStatus = (await BuildModuleStatusesAsync(db, module.TrackId, youthId, ct))
            .FirstOrDefault(item => item.ModuleId == module.Id);
        var curriculumItem = moduleStatus?.Items.FirstOrDefault(item =>
            item.Type is "lesson" or "coding" && item.ItemId == lesson.Id);
        if (moduleStatus is null || moduleStatus.Locked || curriculumItem is null || curriculumItem.Locked)
        {
            throw new ForbiddenException("This lesson is locked.");
        }

        return new AccessibleLesson(lesson, module, moduleStatus, curriculumItem);
    }

    /// <summary>Resolves the track a youth is enrolled in via their cohort, if any.</summary>
    public static async Task<Guid?> ResolveYouthTrackIdAsync(
        IAppDbContext db, Guid youthId, CancellationToken ct)
    {
        var cohortId = await db.Users
            .Where(u => u.Id == youthId)
            .Select(u => u.CohortId)
            .FirstOrDefaultAsync(ct);
        if (cohortId is not { } cid)
        {
            return null;
        }

        return await db.Cohorts
            .Where(c => c.Id == cid)
            .Select(c => c.TrackId)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>Returns the cohort track plus any explicit additional enrollments.</summary>
    public static async Task<IReadOnlyList<Guid>> ResolveYouthTrackIdsAsync(
        IAppDbContext db,
        Guid youthId,
        CancellationToken ct)
    {
        var result = new HashSet<Guid>();
        if (await ResolveYouthTrackIdAsync(db, youthId, ct) is { } cohortTrackId)
        {
            result.Add(cohortTrackId);
        }

        var additional = await db.YouthTrackEnrollments
            .Where(enrollment => enrollment.YouthUserId == youthId)
            .Select(enrollment => enrollment.TrackId)
            .ToListAsync(ct);
        result.UnionWith(additional);
        return result.ToList();
    }

    public static async Task<IReadOnlyList<ModuleStatus>> BuildModuleStatusesAsync(
        IAppDbContext db, Guid trackId, Guid youthId, CancellationToken ct)
    {
        // All modules of the track (any status) — needed to resolve prerequisite names/completeness.
        var allModules = await db.Modules
            .Where(m => m.TrackId == trackId)
            .ToListAsync(ct);
        var moduleIds = allModules.Select(m => m.Id).ToList();

        var lessons = await db.Lessons
            .Where(l => moduleIds.Contains(l.ModuleId)
                && l.Status == PublishStatus.Published
                && l.ArchivedAt == null)
            .ToListAsync(ct);
        var lessonIds = lessons.Select(l => l.Id).ToList();

        var quizzes = await db.Quizzes
            .Where(quiz => quiz.ModuleId != null && moduleIds.Contains(quiz.ModuleId.Value))
            .OrderBy(quiz => quiz.CreatedAt)
            .ThenBy(quiz => quiz.Id)
            .ToListAsync(ct);
        var quizIds = quizzes.Select(quiz => quiz.Id).ToList();

        var progress = await db.LessonProgress
            .Where(p => p.YouthUserId == youthId && lessonIds.Contains(p.LessonId))
            .ToListAsync(ct);
        var quizAttempts = await db.QuizAttempts
            .Where(attempt => attempt.YouthUserId == youthId && quizIds.Contains(attempt.QuizId))
            .ToListAsync(ct);

        var completed = progress
            .Where(p => p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToHashSet();
        var started = progress
            .Where(p => p.Status != ProgressStatus.NotStarted)
            .Select(p => p.LessonId)
            .ToHashSet();

        bool QuizComplete(Quiz quiz) => quizAttempts.Any(attempt =>
            attempt.QuizId == quiz.Id &&
            attempt.IsSubmitted &&
            attempt.ApprovedScore != null &&
            attempt.ApprovedScore >= quiz.PassMark);

        bool ModuleComplete(Guid moduleId)
        {
            var mLessons = lessons.Where(l => l.ModuleId == moduleId).ToList();
            var requiredQuizzes = quizzes
                .Where(quiz => quiz.ModuleId == moduleId && quiz.RequiredToCompleteModule)
                .ToList();
            // A module with no required items is complete because it blocks nothing.
            return mLessons.All(lesson => completed.Contains(lesson.Id)) &&
                   requiredQuizzes.All(QuizComplete);
        }

        var results = new List<ModuleStatus>();
        foreach (var module in allModules
            .Where(m => m.Status == PublishStatus.Published)
            .OrderBy(m => m.Order))
        {
            var mLessons = lessons.Where(l => l.ModuleId == module.Id).ToList();
            var mQuizzes = quizzes.Where(quiz => quiz.ModuleId == module.Id).ToList();
            var requiredQuizzes = mQuizzes.Where(quiz => quiz.RequiredToCompleteModule).ToList();
            var total = mLessons.Count + requiredQuizzes.Count;
            var done = mLessons.Count(l => completed.Contains(l.Id)) + requiredQuizzes.Count(QuizComplete);
            var percent = total == 0 ? 0 : (double)done / total * 100;

            var locked = false;
            string? unlock = null;
            if (module.PrerequisiteModuleId is { } prereqId && !ModuleComplete(prereqId))
            {
                locked = true;
                var prereqName = allModules.FirstOrDefault(m => m.Id == prereqId)?.Name ?? "the prerequisite module";
                unlock = $"Complete {prereqName}";
            }

            string status;
            if (locked)
            {
                status = "locked";
            }
            else if (total > 0 && done == total)
            {
                status = "done";
            }
            else if (mLessons.Any(l => started.Contains(l.Id)) ||
                     mQuizzes.Any(quiz => quizAttempts.Any(attempt => attempt.QuizId == quiz.Id)))
            {
                status = "in-progress";
            }
            else
            {
                status = "not-started";
            }

            var items = new List<CurriculumItem>();
            foreach (var lesson in mLessons.OrderBy(lesson => lesson.Order))
            {
                var firstIncompletePrior = mLessons
                    .Where(prior => prior.Order < lesson.Order && !completed.Contains(prior.Id))
                    .OrderBy(prior => prior.Order)
                    .FirstOrDefault();
                var itemLocked = locked || firstIncompletePrior is not null;
                var itemUnlock = locked
                    ? unlock
                    : firstIncompletePrior is not null
                        ? $"Complete {firstIncompletePrior.Title}"
                        : null;
                var itemStatus = completed.Contains(lesson.Id)
                    ? "done"
                    : started.Contains(lesson.Id)
                        ? "in-progress"
                        : itemLocked
                            ? "locked"
                            : "not-started";
                items.Add(new CurriculumItem(
                    lesson.Id,
                    lesson.IsCodingLesson ? "coding" : "lesson",
                    lesson.Title,
                    lesson.Order,
                    itemStatus,
                    itemLocked,
                    itemUnlock,
                    lesson.EstimatedMinutes));
            }

            var quizOrder = (mLessons.Count == 0 ? 0 : mLessons.Max(lesson => lesson.Order)) + 1;
            foreach (var quiz in mQuizzes)
            {
                var incompleteLessons = mLessons.Any(lesson => !completed.Contains(lesson.Id));
                var itemLocked = locked || incompleteLessons;
                var itemUnlock = locked
                    ? unlock
                    : incompleteLessons
                        ? $"Complete all lessons in {module.Name}"
                        : null;
                var attempted = quizAttempts.Any(attempt => attempt.QuizId == quiz.Id);
                var itemStatus = QuizComplete(quiz)
                    ? "done"
                    : attempted
                        ? "in-progress"
                        : itemLocked
                            ? "locked"
                            : "not-started";
                items.Add(new CurriculumItem(
                    quiz.Id,
                    "quiz",
                    "Module quiz",
                    quizOrder++,
                    itemStatus,
                    itemLocked,
                    itemUnlock,
                    null));
            }

            results.Add(new ModuleStatus(
                module.Id,
                module.Name,
                module.Order,
                status,
                percent,
                locked,
                unlock,
                module.Description,
                module.ScriptureAnchor,
                items,
                done,
                total,
                false));
        }

        var currentIndex = results.FindIndex(module => !module.Locked && module.Status != "done");
        if (currentIndex >= 0)
        {
            results[currentIndex] = results[currentIndex] with { Current = true };
        }

        return results;
    }
}

internal sealed record AccessibleLesson(
    Lesson Lesson,
    Module Module,
    ModuleStatus ModuleStatus,
    CurriculumItem Item);
