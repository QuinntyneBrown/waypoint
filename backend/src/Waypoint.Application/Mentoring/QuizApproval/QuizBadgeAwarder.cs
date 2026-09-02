using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Mentoring.QuizApproval;

/// <summary>Shared logic for awarding "quiz-passed" auto-badges when a quiz attempt is approved with a
/// passing score (L2-LEARN-005 AC2). Awards each matching badge to a youth at most once.</summary>
internal static class QuizBadgeAwarder
{
    public static async Task<IReadOnlyList<NotificationDeliveryPlan>> AwardForPassedQuizAsync(
        IAppDbContext db,
        NotificationDeliveryService notifications,
        Guid quizId,
        Guid youthUserId,
        Guid? awardedByUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var badges = await db.Badges
            .Where(b => b.AutoRule == "quiz-passed"
                        && b.TargetRefId == quizId
                        && b.ArchivedAt == null)
            .ToListAsync(cancellationToken);

        var badgeIds = badges.Select(item => item.Id).ToList();
        var earnedBadgeIds = await db.BadgeAwards
            .Where(item => item.YouthUserId == youthUserId && badgeIds.Contains(item.BadgeId))
            .Select(item => item.BadgeId)
            .ToListAsync(cancellationToken);

        var quiz = await db.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == quizId, cancellationToken);
        if (quiz?.ModuleId is { } moduleId
            && await ModuleIsCompleteAsync(
                db,
                moduleId,
                quizId,
                youthUserId,
                cancellationToken))
        {
            badges.AddRange(await db.Badges
                .Where(item => item.AutoRule == "module-completed"
                    && item.TargetRefId == moduleId
                    && item.ArchivedAt == null)
                .ToListAsync(cancellationToken));
        }
        var deliveryPlans = new List<NotificationDeliveryPlan>();
        foreach (var badge in badges)
        {
            if (earnedBadgeIds.Contains(badge.Id)
                || db.BadgeAwards.Local.Any(item =>
                    item.BadgeId == badge.Id && item.YouthUserId == youthUserId))
            {
                continue;
            }

            db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = badge.Id,
                YouthUserId = youthUserId,
                AwardedByUserId = awardedByUserId,
                AwardedAt = now,
            });
            deliveryPlans.Add(await notifications.PrepareAsync(
                youthUserId,
                "badge-earned",
                $"Badge earned: {badge.Name}",
                string.IsNullOrWhiteSpace(badge.Description)
                    ? "You earned a new badge."
                    : badge.Description,
                "/youth/journey",
                now,
                cancellationToken));
        }

        return deliveryPlans;
    }

    private static async Task<bool> ModuleIsCompleteAsync(
        IAppDbContext db,
        Guid moduleId,
        Guid currentlyPassedQuizId,
        Guid youthUserId,
        CancellationToken cancellationToken)
    {
        if (!await db.Modules.AnyAsync(
                item => item.Id == moduleId && item.Status == PublishStatus.Published,
                cancellationToken))
        {
            return false;
        }

        var lessonIds = await db.Lessons
            .Where(item => item.ModuleId == moduleId
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var completedLessonIds = await db.LessonProgress
            .Where(item => item.YouthUserId == youthUserId
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed)
            .Select(item => item.LessonId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (completedLessonIds.Count != lessonIds.Count)
        {
            return false;
        }

        var requiredQuizzes = await db.Quizzes
            .Where(item => item.ModuleId == moduleId && item.RequiredToCompleteModule)
            .Select(item => new { item.Id, item.PassMark })
            .ToListAsync(cancellationToken);
        foreach (var requiredQuiz in requiredQuizzes)
        {
            if (requiredQuiz.Id == currentlyPassedQuizId)
            {
                continue;
            }

            var trackedPass = db.QuizAttempts.Local.Any(item =>
                item.YouthUserId == youthUserId
                && item.QuizId == requiredQuiz.Id
                && item.IsSubmitted
                && item.ApprovedScore >= requiredQuiz.PassMark);
            if (trackedPass)
            {
                continue;
            }

            if (!await db.QuizAttempts.AnyAsync(item =>
                    item.YouthUserId == youthUserId
                    && item.QuizId == requiredQuiz.Id
                    && item.IsSubmitted
                    && item.ApprovedScore >= requiredQuiz.PassMark,
                    cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    public static async Task DispatchNotificationsAsync(
        NotificationDeliveryService notifications,
        IEnumerable<NotificationDeliveryPlan> deliveryPlans,
        CancellationToken cancellationToken)
    {
        foreach (var plan in deliveryPlans)
        {
            await notifications.DispatchAsync(plan, cancellationToken);
        }
    }
}
