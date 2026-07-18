using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Mentoring.Flags;

/// <summary>
/// Evaluates the three default risk signals. Resolved signals stay suppressed until they clear or a
/// different signal appears, so resolving a flag is durable while genuine recurrence can re-arm it.
/// </summary>
public sealed class AtRiskEvaluator
{
    private const string InactiveKey = "inactive";
    private const string MissedSessionsKey = "missed-sessions";
    private const string ProgressGapKey = "progress-gap";

    private readonly IAppDbContext _db;
    private readonly IAtRiskThresholds _thresholds;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public AtRiskEvaluator(
        IAppDbContext db,
        IAtRiskThresholds thresholds,
        IClock clock,
        IAuditLogger audit)
    {
        _db = db;
        _thresholds = thresholds;
        _clock = clock;
        _audit = audit;
    }

    public async Task EvaluateAsync(
        IReadOnlyCollection<Guid> youthIds,
        Guid? justResolvedFlagId = null,
        CancellationToken cancellationToken = default)
    {
        if (youthIds.Count == 0) return;

        var youth = await _db.Users
            .Where(user => youthIds.Contains(user.Id)
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .Select(user => new YouthActivity(user.Id, user.CohortId, user.CreatedAt, user.LastSignInAt))
            .ToListAsync(cancellationToken);
        if (youth.Count == 0) return;

        var scopedIds = youth.Select(user => user.Id).ToList();
        var missedCounts = await _db.AttendanceRecords
            .Where(record => scopedIds.Contains(record.YouthUserId)
                && record.Status == AttendanceStatus.Absent)
            .GroupBy(record => record.YouthUserId)
            .Select(group => new { YouthId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.YouthId, row => row.Count, cancellationToken);
        var progressGaps = await ProgressGapsAsync(youth, cancellationToken);
        var allFlags = await _db.Flags
            .Where(flag => scopedIds.Contains(flag.YouthUserId))
            .OrderByDescending(flag => flag.FlaggedAt)
            .ToListAsync(cancellationToken);

        var now = _clock.UtcNow;
        var created = new List<Flag>();
        foreach (var person in youth)
        {
            var signals = SignalsFor(person, missedCounts, progressGaps, now);
            var keys = string.Join('|', signals.Select(signal => signal.Key));
            var reason = string.Join(' ', signals.Select(signal => signal.Reason));
            var flags = allFlags.Where(flag => flag.YouthUserId == person.Id).ToList();
            var active = flags.FirstOrDefault(flag => flag.ResolvedAt == null);

            if (active is not null)
            {
                if (signals.Count > 0)
                {
                    active.AutomaticReason = reason;
                    active.AutomaticSignalKeys = keys;
                    active.AutomaticSignalsClearedAt = null;
                }
                else if (active.AutomaticSignalKeys is not null
                    && active.AutomaticSignalsClearedAt is null)
                {
                    active.AutomaticSignalsClearedAt = now;
                }
                continue;
            }

            var latestResolved = flags.FirstOrDefault(flag => flag.ResolvedAt != null);
            if (signals.Count == 0)
            {
                if (latestResolved?.AutomaticSignalKeys is not null
                    && latestResolved.AutomaticSignalsClearedAt is null)
                {
                    latestResolved.AutomaticSignalsClearedAt = now;
                }
                continue;
            }

            if (latestResolved is not null && latestResolved.Id == justResolvedFlagId)
            {
                latestResolved.AutomaticReason = reason;
                latestResolved.AutomaticSignalKeys = keys;
                latestResolved.AutomaticSignalsClearedAt = null;
                continue;
            }

            if (latestResolved?.AutomaticSignalKeys == keys
                && latestResolved.AutomaticSignalsClearedAt is null)
            {
                continue;
            }

            var flag = new Flag
            {
                YouthUserId = person.Id,
                Reason = reason,
                FlaggedByUserId = Guid.Empty,
                FlaggedAt = now,
                IsAutomatic = true,
                AutomaticReason = reason,
                AutomaticSignalKeys = keys,
            };
            _db.Flags.Add(flag);
            allFlags.Add(flag);
            created.Add(flag);
        }

        await _db.SaveChangesAsync(cancellationToken);
        foreach (var flag in created)
        {
            await _audit.LogAsync(
                "YouthAutomaticallyFlagged",
                targetType: "User",
                targetId: flag.YouthUserId,
                isPrivate: true,
                details: flag.Reason,
                cancellationToken: cancellationToken);
        }
    }

    private List<RiskSignal> SignalsFor(
        YouthActivity youth,
        IReadOnlyDictionary<Guid, int> missedCounts,
        IReadOnlyDictionary<Guid, double> progressGaps,
        DateTime now)
    {
        var signals = new List<RiskSignal>();
        var lastActivity = youth.LastSignInAt ?? youth.CreatedAt;
        var inactiveDays = (int)Math.Floor((now - lastActivity).TotalDays);
        if (inactiveDays >= _thresholds.InactiveDays)
        {
            signals.Add(new RiskSignal(
                InactiveKey,
                $"Inactive for {inactiveDays} days (threshold: {_thresholds.InactiveDays})."));
        }

        var missed = missedCounts.GetValueOrDefault(youth.Id);
        if (missed >= _thresholds.MissedSessions)
        {
            signals.Add(new RiskSignal(
                MissedSessionsKey,
                $"Missed {missed} session{(missed == 1 ? string.Empty : "s")} " +
                $"(threshold: {_thresholds.MissedSessions})."));
        }

        var progressGap = progressGaps.GetValueOrDefault(youth.Id);
        if (progressGap > _thresholds.ProgressBelowCohortPercentage)
        {
            signals.Add(new RiskSignal(
                ProgressGapKey,
                $"Progress is {progressGap:0.#}% below the cohort average " +
                $"(threshold: {_thresholds.ProgressBelowCohortPercentage:0.#}%)."));
        }

        return signals;
    }

    private async Task<Dictionary<Guid, double>> ProgressGapsAsync(
        IReadOnlyCollection<YouthActivity> scopedYouth,
        CancellationToken cancellationToken)
    {
        var cohortIds = scopedYouth
            .Where(youth => youth.CohortId != null)
            .Select(youth => youth.CohortId!.Value)
            .Distinct()
            .ToList();
        if (cohortIds.Count == 0) return new Dictionary<Guid, double>();

        var cohorts = await _db.Cohorts
            .Where(cohort => cohortIds.Contains(cohort.Id) && cohort.TrackId != null)
            .Select(cohort => new { cohort.Id, TrackId = cohort.TrackId!.Value })
            .ToListAsync(cancellationToken);
        var cohortTrackIds = cohorts.Select(cohort => cohort.TrackId).Distinct().ToList();
        var moduleRows = await _db.Modules
            .Where(module => cohortTrackIds.Contains(module.TrackId)
                && module.Status == PublishStatus.Published)
            .Select(module => new { module.Id, module.TrackId })
            .ToListAsync(cancellationToken);
        var moduleIds = moduleRows.Select(module => module.Id).ToList();
        var lessonRows = await _db.Lessons
            .Where(lesson => moduleIds.Contains(lesson.ModuleId)
                && lesson.Status == PublishStatus.Published
                && lesson.ArchivedAt == null)
            .Select(lesson => new { lesson.Id, lesson.ModuleId })
            .ToListAsync(cancellationToken);
        if (lessonRows.Count == 0) return new Dictionary<Guid, double>();

        var allCohortYouth = await _db.Users
            .Where(user => user.CohortId != null
                && cohortIds.Contains(user.CohortId.Value)
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .Select(user => new { user.Id, CohortId = user.CohortId!.Value })
            .ToListAsync(cancellationToken);
        var allYouthIds = allCohortYouth.Select(user => user.Id).ToList();
        var lessonIds = lessonRows.Select(lesson => lesson.Id).ToList();
        var completed = await _db.LessonProgress
            .Where(progress => allYouthIds.Contains(progress.YouthUserId)
                && lessonIds.Contains(progress.LessonId)
                && progress.Status == ProgressStatus.Completed)
            .Select(progress => new { progress.YouthUserId, progress.LessonId })
            .ToListAsync(cancellationToken);

        var trackModules = moduleRows
            .GroupBy(module => module.TrackId)
            .ToDictionary(group => group.Key, group => group.Select(module => module.Id).ToHashSet());
        var trackLessons = cohorts
            .Select(cohort => cohort.TrackId)
            .Distinct()
            .ToDictionary(
                trackId => trackId,
                trackId => lessonRows
                    .Where(lesson => trackModules.GetValueOrDefault(trackId)?.Contains(lesson.ModuleId) == true)
                    .Select(lesson => lesson.Id)
                    .ToHashSet());
        var cohortTracks = cohorts.ToDictionary(cohort => cohort.Id, cohort => cohort.TrackId);
        var percentages = new Dictionary<Guid, double>();
        foreach (var person in allCohortYouth)
        {
            if (!cohortTracks.TryGetValue(person.CohortId, out var trackId)
                || !trackLessons.TryGetValue(trackId, out var cohortLessons)
                || cohortLessons.Count == 0)
            {
                continue;
            }
            var completedCount = completed.Count(row =>
                row.YouthUserId == person.Id && cohortLessons.Contains(row.LessonId));
            percentages[person.Id] = (double)completedCount / cohortLessons.Count * 100;
        }

        var gaps = new Dictionary<Guid, double>();
        foreach (var cohort in cohorts)
        {
            var members = allCohortYouth.Where(user => user.CohortId == cohort.Id).ToList();
            var memberPercentages = members
                .Where(member => percentages.ContainsKey(member.Id))
                .Select(member => percentages[member.Id])
                .ToList();
            if (memberPercentages.Count == 0) continue;
            var average = memberPercentages.Average();
            foreach (var member in members)
            {
                if (percentages.TryGetValue(member.Id, out var percent))
                {
                    gaps[member.Id] = Math.Max(0, average - percent);
                }
            }
        }
        return gaps;
    }

    private sealed record YouthActivity(Guid Id, Guid? CohortId, DateTime CreatedAt, DateTime? LastSignInAt);
    private sealed record RiskSignal(string Key, string Reason);
}
