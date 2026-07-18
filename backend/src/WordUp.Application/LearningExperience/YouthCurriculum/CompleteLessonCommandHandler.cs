using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public class CompleteLessonCommandHandler : IRequestHandler<CompleteLessonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CompleteLessonCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();

        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ArchivedAt == null, cancellationToken)
            ?? throw new NotFoundException("Lesson not found.");

        // The lesson must be published.
        if (lesson.Status != PublishStatus.Published)
        {
            throw new ForbiddenException("This lesson is locked.");
        }

        var module = await _db.Modules.FirstOrDefaultAsync(m => m.Id == lesson.ModuleId, cancellationToken)
            ?? throw new NotFoundException("Module not found.");

        // The module must not be locked (its prerequisite must be complete for this youth).
        if (module.PrerequisiteModuleId is { } prereqId
            && !await ModuleCompleteAsync(prereqId, youthId, cancellationToken))
        {
            throw new ForbiddenException("This lesson is locked.");
        }

        // All prior published lessons in this module must already be complete.
        var priorLessonIds = await _db.Lessons
            .Where(l => l.ModuleId == module.Id
                && l.Status == PublishStatus.Published
                && l.ArchivedAt == null
                && l.Order < lesson.Order)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        if (priorLessonIds.Count > 0)
        {
            var completedPrior = await _db.LessonProgress
                .Where(p => p.YouthUserId == youthId
                    && priorLessonIds.Contains(p.LessonId)
                    && p.Status == ProgressStatus.Completed)
                .Select(p => p.LessonId)
                .ToListAsync(cancellationToken);

            if (completedPrior.Distinct().Count() < priorLessonIds.Count)
            {
                throw new ForbiddenException("This lesson is locked.");
            }
        }

        var now = _clock.UtcNow;

        var existing = await _db.LessonProgress
            .FirstOrDefaultAsync(p => p.YouthUserId == youthId && p.LessonId == lesson.Id, cancellationToken);
        if (existing is null)
        {
            _db.LessonProgress.Add(new LessonProgress
            {
                YouthUserId = youthId,
                LessonId = lesson.Id,
                Status = ProgressStatus.Completed,
                CompletedAt = now,
                UpdatedAt = now,
            });
        }
        else
        {
            existing.Status = ProgressStatus.Completed;
            existing.CompletedAt = now;
            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await AwardLessonCompletedBadgesAsync(lesson.Id, youthId, now, cancellationToken);

        await _audit.LogAsync("LessonCompleted", youthId, "Lesson", lesson.Id, cancellationToken: cancellationToken);
    }

    private async Task<bool> ModuleCompleteAsync(Guid moduleId, Guid youthId, CancellationToken ct)
    {
        var lessonIds = await _db.Lessons
            .Where(l => l.ModuleId == moduleId
                && l.Status == PublishStatus.Published
                && l.ArchivedAt == null)
            .Select(l => l.Id)
            .ToListAsync(ct);

        // A module with no published lessons is treated as complete.
        if (lessonIds.Count == 0)
        {
            return true;
        }

        var completed = await _db.LessonProgress
            .Where(p => p.YouthUserId == youthId
                && lessonIds.Contains(p.LessonId)
                && p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToListAsync(ct);

        return completed.Distinct().Count() >= lessonIds.Count;
    }

    private async Task AwardLessonCompletedBadgesAsync(Guid lessonId, Guid youthId, DateTime now, CancellationToken ct)
    {
        var badges = await _db.Badges
            .Where(b => b.AutoRule == "lesson-completed"
                && b.ArchivedAt == null
                && (b.TargetRefId == lessonId || b.TargetRefId == null))
            .ToListAsync(ct);

        if (badges.Count == 0)
        {
            return;
        }

        var awarded = false;
        foreach (var badge in badges)
        {
            var already = await _db.BadgeAwards
                .AnyAsync(a => a.BadgeId == badge.Id && a.YouthUserId == youthId, ct);
            if (already)
            {
                continue;
            }

            _db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = badge.Id,
                YouthUserId = youthId,
                AwardedByUserId = null,
                AwardedAt = now,
            });
            awarded = true;
        }

        if (awarded)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
