using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ReportingAndData.Reports;

internal static class ReportData
{
    public static DateTime? EndExclusive(DateTime? to) =>
        to is null ? null : to.Value.TimeOfDay == TimeSpan.Zero ? to.Value.Date.AddDays(1) : to;

    public static async Task<List<User>> YouthAsync(
        IAppDbContext db,
        ICurrentUser currentUser,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().Where(item => item.Role == Roles.Youth);
        if (currentUser.Role == Roles.Mentor)
        {
            if (filter.MentorId is Guid requestedMentor && requestedMentor != currentUser.UserId)
                throw new ForbiddenException("Mentors can only report on their assigned youth.");
            query = query.Where(item => item.MentorId == currentUser.UserId);
        }
        else if (filter.MentorId is Guid mentorId)
        {
            query = query.Where(item => item.MentorId == mentorId);
        }

        if (filter.CohortId is Guid cohortId)
            query = query.Where(item => item.CohortId == cohortId);
        if (filter.TrackId is Guid trackId)
        {
            query = query.Where(item =>
                db.Cohorts.Any(cohort => cohort.Id == item.CohortId && cohort.TrackId == trackId)
                || db.YouthTrackEnrollments.Any(enrollment =>
                    enrollment.YouthUserId == item.Id && enrollment.TrackId == trackId));
        }

        return await query.OrderBy(item => item.DisplayName).ToListAsync(cancellationToken);
    }

    public static async Task<List<Guid>> TrackIdsForYouthAsync(
        IAppDbContext db,
        User youth,
        Guid? selectedTrackId,
        CancellationToken cancellationToken)
    {
        if (selectedTrackId is Guid trackId) return [trackId];

        var trackIds = await db.YouthTrackEnrollments.AsNoTracking()
            .Where(item => item.YouthUserId == youth.Id)
            .Select(item => item.TrackId)
            .ToListAsync(cancellationToken);
        if (youth.CohortId is Guid cohortId)
        {
            var cohortTrackId = await db.Cohorts.AsNoTracking()
                .Where(item => item.Id == cohortId)
                .Select(item => item.TrackId)
                .FirstOrDefaultAsync(cancellationToken);
            if (cohortTrackId is Guid primaryTrackId) trackIds.Add(primaryTrackId);
        }
        return trackIds.Distinct().ToList();
    }

    public static async Task<IReadOnlyList<ModuleReport>> ModulesForYouthAsync(
        IAppDbContext db,
        User youth,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var trackIds = await TrackIdsForYouthAsync(db, youth, filter.TrackId, cancellationToken);
        if (trackIds.Count == 0) return [];

        var modules = await db.Modules.AsNoTracking()
            .Where(item => trackIds.Contains(item.TrackId) && item.Status == PublishStatus.Published)
            .OrderBy(item => item.Order)
            .Select(item => new { item.Id, item.Name })
            .ToListAsync(cancellationToken);
        var moduleIds = modules.Select(item => item.Id).ToList();
        var lessons = await db.Lessons.AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => new { item.Id, item.ModuleId })
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(item => item.Id).ToList();
        var end = EndExclusive(filter.To);
        var completedIds = await db.LessonProgress.AsNoTracking()
            .Where(item => item.YouthUserId == youth.Id
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed
                && (filter.From == null || item.CompletedAt >= filter.From)
                && (end == null || item.CompletedAt < end))
            .Select(item => item.LessonId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var completed = completedIds.ToHashSet();

        return modules.Select(module =>
        {
            var moduleLessons = lessons.Where(item => item.ModuleId == module.Id).ToList();
            var done = moduleLessons.Count(item => completed.Contains(item.Id));
            return new ModuleReport(
                module.Id,
                module.Name,
                done,
                moduleLessons.Count,
                moduleLessons.Count == 0 ? 0d : done / (double)moduleLessons.Count);
        }).ToList();
    }

    public static async Task<IReadOnlyList<ModuleReport>> ModulesForCohortAsync(
        IAppDbContext db,
        IReadOnlyList<User> youth,
        ReportFilter filter,
        Guid? defaultTrackId,
        CancellationToken cancellationToken)
    {
        var trackId = filter.TrackId ?? defaultTrackId;
        if (trackId is null || youth.Count == 0) return [];

        var modules = await db.Modules.AsNoTracking()
            .Where(item => item.TrackId == trackId && item.Status == PublishStatus.Published)
            .OrderBy(item => item.Order)
            .Select(item => new { item.Id, item.Name })
            .ToListAsync(cancellationToken);
        var moduleIds = modules.Select(item => item.Id).ToList();
        var lessons = await db.Lessons.AsNoTracking()
            .Where(item => moduleIds.Contains(item.ModuleId)
                && item.Status == PublishStatus.Published
                && item.ArchivedAt == null)
            .Select(item => new { item.Id, item.ModuleId })
            .ToListAsync(cancellationToken);
        var lessonIds = lessons.Select(item => item.Id).ToList();
        var youthIds = youth.Select(item => item.Id).ToList();
        var end = EndExclusive(filter.To);
        var progress = await db.LessonProgress.AsNoTracking()
            .Where(item => youthIds.Contains(item.YouthUserId)
                && lessonIds.Contains(item.LessonId)
                && item.Status == ProgressStatus.Completed
                && (filter.From == null || item.CompletedAt >= filter.From)
                && (end == null || item.CompletedAt < end))
            .Select(item => new { item.YouthUserId, item.LessonId })
            .Distinct()
            .ToListAsync(cancellationToken);
        var completed = progress.Select(item => (item.YouthUserId, item.LessonId)).ToHashSet();

        return modules.Select(module =>
        {
            var moduleLessonIds = lessons
                .Where(item => item.ModuleId == module.Id)
                .Select(item => item.Id)
                .ToList();
            var total = moduleLessonIds.Count * youth.Count;
            var done = youth.Sum(person => moduleLessonIds.Count(lessonId => completed.Contains((person.Id, lessonId))));
            return new ModuleReport(module.Id, module.Name, done, total, total == 0 ? 0d : done / (double)total);
        }).ToList();
    }

    public static async Task<double> CompletionRateAsync(
        IAppDbContext db,
        IReadOnlyList<User> youth,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        if (youth.Count == 0) return 0d;
        var rates = new List<double>(youth.Count);
        foreach (var person in youth)
        {
            var modules = await ModulesForYouthAsync(db, person, filter, cancellationToken);
            var completed = modules.Sum(item => item.CompletedLessons);
            var total = modules.Sum(item => item.TotalLessons);
            rates.Add(total == 0 ? 0d : completed / (double)total);
        }
        return rates.Average();
    }

    public static async Task<AttendanceBreakdown> AttendanceAsync(
        IAppDbContext db,
        IReadOnlyList<Guid> youthIds,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        if (youthIds.Count == 0) return new AttendanceBreakdown(0, 0, 0, 0, 0, 0d);
        var end = EndExclusive(filter.To);
        var statuses = await db.AttendanceRecords.AsNoTracking()
            .Where(item => youthIds.Contains(item.YouthUserId)
                && (filter.From == null || item.RecordedAt >= filter.From)
                && (end == null || item.RecordedAt < end))
            .Select(item => item.Status)
            .ToListAsync(cancellationToken);
        var present = statuses.Count(item => item == AttendanceStatus.Present);
        var late = statuses.Count(item => item == AttendanceStatus.Late);
        var excused = statuses.Count(item => item == AttendanceStatus.Excused);
        var absent = statuses.Count(item => item == AttendanceStatus.Absent);
        var attended = present + late;
        return new AttendanceBreakdown(
            present,
            late,
            excused,
            absent,
            statuses.Count,
            statuses.Count == 0 ? 0d : attended / (double)statuses.Count);
    }

    public static async Task<CohortMetric> CohortMetricAsync(
        IAppDbContext db,
        Cohort cohort,
        IReadOnlyList<User> youth,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var completion = await CompletionRateAsync(db, youth, filter with { CohortId = cohort.Id }, cancellationToken);
        var attendance = await AttendanceAsync(db, youth.Select(item => item.Id).ToList(), filter, cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var atRisk = youthIds.Count == 0
            ? 0
            : await db.Flags.AsNoTracking()
                .Where(item => youthIds.Contains(item.YouthUserId) && item.ResolvedAt == null)
                .Select(item => item.YouthUserId)
                .Distinct()
                .CountAsync(cancellationToken);
        var active = youth.Count(item => item.Status == UserStatus.Active);
        return new CohortMetric(
            cohort.Id,
            cohort.Name,
            youth.Count,
            completion,
            attendance.Rate,
            youth.Count == 0 ? 0d : active / (double)youth.Count,
            atRisk);
    }

    public static async Task<IReadOnlyList<StaffActivityRow>> StaffAsync(
        IAppDbContext db,
        ICurrentUser currentUser,
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var youth = await YouthAsync(db, currentUser, filter, cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var mentorQuery = db.Users.AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active);
        if (currentUser.Role == Roles.Mentor)
            mentorQuery = mentorQuery.Where(item => item.Id == currentUser.UserId);
        else if (filter.MentorId is Guid mentorId)
            mentorQuery = mentorQuery.Where(item => item.Id == mentorId);
        else if (filter.CohortId != null || filter.TrackId != null)
        {
            var scopedMentorIds = youth.Where(item => item.MentorId != null)
                .Select(item => item.MentorId!.Value)
                .Distinct()
                .ToList();
            mentorQuery = mentorQuery.Where(item => scopedMentorIds.Contains(item.Id));
        }
        var mentors = await mentorQuery.OrderBy(item => item.DisplayName).ToListAsync(cancellationToken);
        var mentorIds = mentors.Select(item => item.Id).ToList();
        var end = EndExclusive(filter.To);
        var checkIns = youthIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await db.CheckIns.AsNoTracking()
                .Where(item => youthIds.Contains(item.YouthUserId)
                    && mentorIds.Contains(item.MentorUserId)
                    && (filter.From == null || item.Date >= filter.From)
                    && (end == null || item.Date < end))
                .GroupBy(item => item.MentorUserId)
                .Select(group => new { MentorId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.MentorId, item => item.Count, cancellationToken);
        var reviews = youthIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await db.Submissions.AsNoTracking()
                .Where(item => youthIds.Contains(item.YouthUserId)
                    && item.ReviewedByUserId != null
                    && mentorIds.Contains(item.ReviewedByUserId.Value)
                    && (filter.From == null || item.ReviewedAt >= filter.From)
                    && (end == null || item.ReviewedAt < end))
                .GroupBy(item => item.ReviewedByUserId!.Value)
                .Select(group => new { MentorId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.MentorId, item => item.Count, cancellationToken);

        return mentors.Select(mentor =>
        {
            var caseload = youth.Count(item => item.Status == UserStatus.Active && item.MentorId == mentor.Id);
            var loadBalance = caseload < StaffActivityReportQueryHandler.TargetMinimum
                ? "Under target"
                : caseload > StaffActivityReportQueryHandler.TargetMaximum
                    ? "Overloaded"
                    : "Balanced";
            return new StaffActivityRow(
                mentor.Id,
                mentor.DisplayName,
                mentor.Email,
                caseload,
                checkIns.GetValueOrDefault(mentor.Id),
                reviews.GetValueOrDefault(mentor.Id),
                StaffActivityReportQueryHandler.TargetMinimum,
                StaffActivityReportQueryHandler.TargetMaximum,
                loadBalance,
                caseload > StaffActivityReportQueryHandler.TargetMaximum);
        }).ToList();
    }
}
