using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Mentoring.QuizApproval;

/// <summary>Shared logic for awarding "quiz-passed" auto-badges when a quiz attempt is approved with a
/// passing score (L2-LEARN-005 AC2). Awards each matching badge to a youth at most once.</summary>
internal static class QuizBadgeAwarder
{
    public static async Task AwardForPassedQuizAsync(
        IAppDbContext db,
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

        foreach (var badge in badges)
        {
            var alreadyAwarded = await db.BadgeAwards
                .AnyAsync(a => a.BadgeId == badge.Id && a.YouthUserId == youthUserId, cancellationToken);
            if (alreadyAwarded)
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
        }
    }
}
