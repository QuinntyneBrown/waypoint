using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ReportingAndData.Reports;

public record ReportsHubQuery(ReportFilter Filter) : IRequest<ReportsHub>;

public record ReportsHub(
    DateTime GeneratedAt,
    int RecordCount,
    ReportKpis Kpis,
    AttendanceBreakdown Attendance,
    IReadOnlyList<CohortMetric> Cohorts,
    IReadOnlyList<StaffActivityRow> Staff);

public sealed class ReportsHubQueryHandler : IRequestHandler<ReportsHubQuery, ReportsHub>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public ReportsHubQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<ReportsHub> Handle(ReportsHubQuery request, CancellationToken cancellationToken)
    {
        var youth = await ReportData.YouthAsync(_db, _currentUser, request.Filter, cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var attendance = await ReportData.AttendanceAsync(_db, youthIds, request.Filter, cancellationToken);
        var completion = await ReportData.CompletionRateAsync(_db, youth, request.Filter, cancellationToken);
        var cohortIds = youth.Where(item => item.CohortId != null)
            .Select(item => item.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohorts = await _db.Cohorts.AsNoTracking()
            .Where(item => cohortIds.Contains(item.Id))
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var cohortMetrics = new List<CohortMetric>(cohorts.Count);
        foreach (var cohort in cohorts)
        {
            cohortMetrics.Add(await ReportData.CohortMetricAsync(
                _db,
                cohort,
                youth.Where(item => item.CohortId == cohort.Id).ToList(),
                request.Filter,
                cancellationToken));
        }
        var activeYouth = youth.Count(item => item.Status == UserStatus.Active);
        var staff = await ReportData.StaffAsync(_db, _currentUser, request.Filter, cancellationToken);
        var kpis = new ReportKpis(
            activeYouth,
            cohortMetrics.Count,
            completion,
            attendance.Rate,
            youth.Count == 0 ? 0d : activeYouth / (double)youth.Count,
            youth.Where(item => item.Status == UserStatus.Active && item.MentorId != null)
                .Select(item => item.MentorId!.Value)
                .Distinct()
                .Count());
        return new ReportsHub(_clock.UtcNow, youth.Count, kpis, attendance, cohortMetrics, staff);
    }
}
