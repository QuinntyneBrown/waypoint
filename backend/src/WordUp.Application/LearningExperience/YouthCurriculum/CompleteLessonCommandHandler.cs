using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public class CompleteLessonCommandHandler : IRequestHandler<CompleteLessonCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IChangePublisher _publisher;
    private readonly NotificationDeliveryService _notifications;

    public CompleteLessonCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IChangePublisher publisher,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _publisher = publisher;
        _notifications = notifications;
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
        var enrolledTrackIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
            _db,
            youthId,
            cancellationToken);
        if (module.Status != PublishStatus.Published || !enrolledTrackIds.Contains(module.TrackId))
        {
            throw new ForbiddenException("This lesson is locked.");
        }

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

        await AwardCompletionBadgesAsync(lesson, module, youthId, now, cancellationToken);

        await _audit.LogAsync("LessonCompleted", youthId, "Lesson", lesson.Id, cancellationToken: cancellationToken);
        var cohortId = await _db.Users
            .Where(user => user.Id == youthId)
            .Select(user => user.CohortId)
            .FirstOrDefaultAsync(cancellationToken);
        if (cohortId is { } progressCohortId)
        {
            await _publisher.PublishToRoleAsync(
                Roles.ProgramAdministrator,
                "progressChanged",
                new { cohortId = progressCohortId, youthId },
                cancellationToken);
        }
    }

    private async Task<bool> ModuleCompleteAsync(Guid moduleId, Guid youthId, CancellationToken ct)
    {
        var lessonIds = await _db.Lessons
            .Where(l => l.ModuleId == moduleId
                && l.Status == PublishStatus.Published
                && l.ArchivedAt == null)
            .Select(l => l.Id)
            .ToListAsync(ct);

        var completed = await _db.LessonProgress
            .Where(p => p.YouthUserId == youthId
                && lessonIds.Contains(p.LessonId)
                && p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToListAsync(ct);

        if (completed.Distinct().Count() < lessonIds.Count)
        {
            return false;
        }

        var requiredQuizzes = await _db.Quizzes
            .Where(item => item.ModuleId == moduleId && item.RequiredToCompleteModule)
            .Select(item => new { item.Id, item.PassMark })
            .ToListAsync(ct);
        if (requiredQuizzes.Count == 0)
        {
            return true;
        }

        var requiredQuizIds = requiredQuizzes.Select(item => item.Id).ToList();
        var approvedAttempts = await _db.QuizAttempts
            .Where(item => item.YouthUserId == youthId
                && requiredQuizIds.Contains(item.QuizId)
                && item.IsSubmitted
                && item.ApprovedScore != null)
            .Select(item => new { item.QuizId, Score = item.ApprovedScore!.Value })
            .ToListAsync(ct);
        return requiredQuizzes.All(quiz =>
            approvedAttempts.Any(attempt => attempt.QuizId == quiz.Id && attempt.Score >= quiz.PassMark));
    }

    private async Task AwardCompletionBadgesAsync(
        Lesson lesson,
        Module module,
        Guid youthId,
        DateTime now,
        CancellationToken ct)
    {
        var badges = await _db.Badges
            .Where(b => b.AutoRule == "lesson-completed"
                && b.ArchivedAt == null
                && (b.TargetRefId == lesson.Id || b.TargetRefId == null))
            .ToListAsync(ct);
        if (await ModuleCompleteAsync(module.Id, youthId, ct))
        {
            badges.AddRange(await _db.Badges
                .Where(item => item.AutoRule == "module-completed"
                    && item.ArchivedAt == null
                    && item.TargetRefId == module.Id)
                .ToListAsync(ct));
        }

        if (badges.Count == 0)
        {
            return;
        }

        var badgeIds = badges.Select(item => item.Id).Distinct().ToList();
        var earnedBadgeIds = await _db.BadgeAwards
            .Where(item => item.YouthUserId == youthId && badgeIds.Contains(item.BadgeId))
            .Select(item => item.BadgeId)
            .ToListAsync(ct);
        var notificationPlans = new List<NotificationDeliveryPlan>();
        foreach (var badge in badges.Where(item => !earnedBadgeIds.Contains(item.Id)))
        {
            _db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = badge.Id,
                YouthUserId = youthId,
                AwardedByUserId = null,
                AwardedAt = now,
            });
            notificationPlans.Add(await _notifications.PrepareAsync(
                youthId,
                "badge-earned",
                $"Badge earned: {badge.Name}",
                string.IsNullOrWhiteSpace(badge.Description)
                    ? "You earned a new badge."
                    : badge.Description,
                "/youth/journey",
                now,
                ct));
        }

        if (notificationPlans.Count == 0)
        {
            return;
        }

        await _db.SaveChangesAsync(ct);
        foreach (var plan in notificationPlans)
        {
            await _notifications.DispatchAsync(plan, ct);
        }
    }
}
