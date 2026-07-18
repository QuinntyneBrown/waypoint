using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.YouthCurriculum;

/// <summary>
/// Single source of truth for per-module status + gating for a youth (L2-LEARN-001/002/009/010).
/// A module is complete for a youth when all of its published lessons have a Completed LessonProgress.
/// A module is locked when its prerequisite module is not yet complete for that youth.
/// </summary>
internal static class CurriculumProgressCalculator
{
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

        var progress = await db.LessonProgress
            .Where(p => p.YouthUserId == youthId && lessonIds.Contains(p.LessonId))
            .ToListAsync(ct);

        var completed = progress
            .Where(p => p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToHashSet();
        var started = progress
            .Where(p => p.Status != ProgressStatus.NotStarted)
            .Select(p => p.LessonId)
            .ToHashSet();

        bool ModuleComplete(Guid moduleId)
        {
            var mLessons = lessons.Where(l => l.ModuleId == moduleId).ToList();
            // A module with no published lessons is treated as complete (nothing blocks progression).
            return mLessons.All(l => completed.Contains(l.Id));
        }

        var results = new List<ModuleStatus>();
        foreach (var module in allModules
            .Where(m => m.Status == PublishStatus.Published)
            .OrderBy(m => m.Order))
        {
            var mLessons = lessons.Where(l => l.ModuleId == module.Id).ToList();
            var total = mLessons.Count;
            var done = mLessons.Count(l => completed.Contains(l.Id));
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
            else if (mLessons.Any(l => started.Contains(l.Id)))
            {
                status = "in-progress";
            }
            else
            {
                status = "not-started";
            }

            results.Add(new ModuleStatus(module.Id, module.Name, module.Order, status, percent, locked, unlock));
        }

        return results;
    }
}
