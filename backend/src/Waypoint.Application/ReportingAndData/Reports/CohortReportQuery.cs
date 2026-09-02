using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ReportingAndData.Reports;

public record CohortReportQuery(
    Guid CohortId,
    ReportFilter? Filter = null,
    Guid? CompareCohortId = null) : IRequest<CohortReport>;

public record CohortYouthSummary(
    Guid YouthId,
    string YouthName,
    double CompletionRate,
    double AttendanceRate,
    string Status);

public record CohortComparison(
    string Label,
    double CompletionRate,
    double AttendanceRate,
    double RetentionRate);

public record CohortReport(
    Guid CohortId,
    string CohortName,
    int Enrolment,
    double AvgCompletion,
    double AttendanceRate,
    double RetentionRate,
    int StatusActive,
    int StatusAtRisk,
    IReadOnlyList<ModuleReport> Modules,
    IReadOnlyList<CohortYouthSummary> Youth,
    IReadOnlyList<CohortComparison> Comparisons);

public class CohortReportQueryHandler : IRequestHandler<CohortReportQuery, CohortReport>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CohortReportQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CohortReport> Handle(CohortReportQuery request, CancellationToken cancellationToken)
    {
        var filter = (request.Filter ?? new ReportFilter()) with { CohortId = request.CohortId };
        var cohort = await _db.Cohorts.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CohortId, cancellationToken);
        if (cohort is null)
        {
            return new CohortReport(
                request.CohortId,
                "Cohort",
                0,
                0d,
                0d,
                0d,
                0,
                0,
                [],
                [],
                [new CohortComparison("Program average", 0d, 0d, 0d)]);
        }

        if (_currentUser.Role == Roles.Mentor)
        {
            var ownsCohort = cohort.MentorId == _currentUser.UserId
                || await _db.CohortMentors.AsNoTracking().AnyAsync(
                    item => item.CohortId == cohort.Id && item.MentorId == _currentUser.UserId,
                    cancellationToken)
                || await _db.Users.AsNoTracking().AnyAsync(
                    item => item.CohortId == cohort.Id && item.MentorId == _currentUser.UserId,
                    cancellationToken);
            if (!ownsCohort) throw new ForbiddenException("This cohort is outside your scope.");
        }

        var youth = await ReportData.YouthAsync(_db, _currentUser, filter, cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var attendance = await ReportData.AttendanceAsync(_db, youthIds, filter, cancellationToken);
        var completion = await ReportData.CompletionRateAsync(_db, youth, filter, cancellationToken);
        var modules = await ReportData.ModulesForCohortAsync(
            _db,
            youth,
            filter,
            cohort.TrackId,
            cancellationToken);
        var atRiskIds = youthIds.Count == 0
            ? new HashSet<Guid>()
            : (await _db.Flags.AsNoTracking()
                .Where(item => youthIds.Contains(item.YouthUserId) && item.ResolvedAt == null)
                .Select(item => item.YouthUserId)
                .Distinct()
                .ToListAsync(cancellationToken)).ToHashSet();
        var summaries = new List<CohortYouthSummary>(youth.Count);
        foreach (var person in youth)
        {
            var personModules = await ReportData.ModulesForYouthAsync(_db, person, filter, cancellationToken);
            var total = personModules.Sum(item => item.TotalLessons);
            var done = personModules.Sum(item => item.CompletedLessons);
            var personAttendance = await ReportData.AttendanceAsync(_db, [person.Id], filter, cancellationToken);
            var status = person.Status != UserStatus.Active
                ? "Inactive"
                : atRiskIds.Contains(person.Id)
                    ? "At risk"
                    : "Active";
            summaries.Add(new CohortYouthSummary(
                person.Id,
                person.DisplayName,
                total == 0 ? 0d : done / (double)total,
                personAttendance.Rate,
                status));
        }

        var programFilter = filter with { CohortId = null };
        var programYouth = await ReportData.YouthAsync(_db, _currentUser, programFilter, cancellationToken);
        var programCompletion = await ReportData.CompletionRateAsync(_db, programYouth, programFilter, cancellationToken);
        var programAttendance = await ReportData.AttendanceAsync(
            _db,
            programYouth.Select(item => item.Id).ToList(),
            programFilter,
            cancellationToken);
        var programActive = programYouth.Count(item => item.Status == UserStatus.Active);
        var comparisons = new List<CohortComparison>
        {
            new(
                "Program average",
                programCompletion,
                programAttendance.Rate,
                programYouth.Count == 0 ? 0d : programActive / (double)programYouth.Count),
        };

        if (request.CompareCohortId is Guid compareId && compareId != cohort.Id)
        {
            var comparison = await ComparisonAsync(compareId, filter, cancellationToken);
            if (comparison is not null) comparisons.Add(comparison);
        }

        var previous = await _db.Cohorts.AsNoTracking()
            .Where(item => item.Id != cohort.Id && item.TermEnd < cohort.TermStart)
            .OrderByDescending(item => item.TermEnd)
            .FirstOrDefaultAsync(cancellationToken);
        if (previous is not null)
        {
            var previousComparison = await ComparisonAsync(previous.Id, filter, cancellationToken, "Previous term");
            if (previousComparison is not null) comparisons.Add(previousComparison);
        }

        var statusActive = summaries.Count(item => item.Status == "Active");
        var statusAtRisk = summaries.Count(item => item.Status == "At risk");
        var active = statusActive + statusAtRisk;
        return new CohortReport(
            cohort.Id,
            cohort.Name,
            youth.Count,
            completion,
            attendance.Rate,
            youth.Count == 0 ? 0d : active / (double)youth.Count,
            statusActive,
            statusAtRisk,
            modules,
            summaries,
            comparisons);
    }

    private async Task<CohortComparison?> ComparisonAsync(
        Guid cohortId,
        ReportFilter filter,
        CancellationToken cancellationToken,
        string? label = null)
    {
        var cohort = await _db.Cohorts.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == cohortId, cancellationToken);
        if (cohort is null) return null;
        if (_currentUser.Role == Roles.Mentor)
        {
            var allowed = await _db.Users.AsNoTracking().AnyAsync(
                item => item.CohortId == cohortId && item.MentorId == _currentUser.UserId,
                cancellationToken);
            if (!allowed) return null;
        }
        var comparisonFilter = filter with { CohortId = cohortId };
        var youth = await ReportData.YouthAsync(_db, _currentUser, comparisonFilter, cancellationToken);
        var metric = await ReportData.CohortMetricAsync(_db, cohort, youth, comparisonFilter, cancellationToken);
        return new CohortComparison(
            label ?? $"Compared cohort: {cohort.Name}",
            metric.CompletionRate,
            metric.AttendanceRate,
            metric.RetentionRate);
    }
}
