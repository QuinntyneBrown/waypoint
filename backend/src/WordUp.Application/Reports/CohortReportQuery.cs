using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Reports;

public record CohortReportQuery(Guid CohortId) : IRequest<CohortReport>;

public record CohortReport(
    Guid CohortId,
    int Enrolment,
    double AvgCompletion,
    double AttendanceRate,
    int StatusActive,
    int StatusAtRisk);

public class CohortReportQueryHandler : IRequestHandler<CohortReportQuery, CohortReport>
{
    private readonly IAppDbContext _db;

    public CohortReportQueryHandler(IAppDbContext db) => _db = db;

    public async Task<CohortReport> Handle(CohortReportQuery request, CancellationToken cancellationToken)
    {
        var youthIds = await _db.Users
            .Where(u => u.CohortId == request.CohortId && u.Role == Roles.Youth)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var enrolment = youthIds.Count;
        if (enrolment == 0)
        {
            // Empty state (L2-REPORT-001): no youth in the cohort => zeros, not an error.
            return new CohortReport(request.CohortId, 0, 0d, 0d, 0, 0);
        }

        var totalLessons = await _db.Lessons.CountAsync(cancellationToken);

        var completedByYouth = await _db.LessonProgress
            .Where(p => youthIds.Contains(p.YouthUserId) && p.Status == ProgressStatus.Completed)
            .GroupBy(p => p.YouthUserId)
            .Select(g => new { YouthId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var completedMap = completedByYouth.ToDictionary(x => x.YouthId, x => x.Count);

        var avgCompletion = 0d;
        if (totalLessons > 0)
        {
            var sumRatio = youthIds.Sum(id => (completedMap.TryGetValue(id, out var c) ? c : 0) / (double)totalLessons);
            avgCompletion = sumRatio / enrolment;
        }

        var attendanceStatuses = await _db.AttendanceRecords
            .Where(a => youthIds.Contains(a.YouthUserId))
            .Select(a => a.Status)
            .ToListAsync(cancellationToken);
        var attendanceRate = attendanceStatuses.Count == 0
            ? 0d
            : attendanceStatuses.Count(s => s == AttendanceStatus.Present || s == AttendanceStatus.Late)
              / (double)attendanceStatuses.Count;

        // At-risk = the youth has an open (unresolved) flag.
        var atRiskYouth = await _db.Flags
            .Where(f => youthIds.Contains(f.YouthUserId) && f.ResolvedAt == null)
            .Select(f => f.YouthUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var statusAtRisk = atRiskYouth.Count;
        var statusActive = enrolment - statusAtRisk;

        return new CohortReport(request.CohortId, enrolment, avgCompletion, attendanceRate, statusActive, statusAtRisk);
    }
}
