using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ReportingAndData.DataTransfer;

public record ExportCohortReportQuery(Guid CohortId) : IRequest<CsvFile>;

public record CsvFile(string FileName, string Content);

public class ExportCohortReportQueryHandler : IRequestHandler<ExportCohortReportQuery, CsvFile>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public ExportCohortReportQueryHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<CsvFile> Handle(ExportCohortReportQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == Roles.Mentor)
        {
            var allowed = await _db.Cohorts.AsNoTracking().AnyAsync(
                    item => item.Id == request.CohortId && item.MentorId == _currentUser.UserId,
                    cancellationToken)
                || await _db.CohortMentors.AsNoTracking().AnyAsync(
                    item => item.CohortId == request.CohortId && item.MentorId == _currentUser.UserId,
                    cancellationToken)
                || await _db.Users.AsNoTracking().AnyAsync(
                    item => item.CohortId == request.CohortId && item.MentorId == _currentUser.UserId,
                    cancellationToken);
            if (!allowed) throw new ForbiddenException("This cohort is outside your scope.");
        }

        var youth = await _db.Users
            .Where(u => u.CohortId == request.CohortId && u.Role == Roles.Youth)
            .OrderBy(u => u.DisplayName)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.Append(Csv.ToRow(new[] { "YouthId", "Name", "LessonsCompleted", "AttendancePresent", "AttendanceTotal" }));
        sb.Append("\r\n");

        foreach (var y in youth)
        {
            var lessonsCompleted = await _db.LessonProgress
                .CountAsync(p => p.YouthUserId == y.Id && p.Status == ProgressStatus.Completed, cancellationToken);

            var attendance = await _db.AttendanceRecords
                .Where(a => a.YouthUserId == y.Id)
                .Select(a => a.Status)
                .ToListAsync(cancellationToken);
            var total = attendance.Count;
            var present = attendance.Count(s => s == AttendanceStatus.Present || s == AttendanceStatus.Late);

            sb.Append(Csv.ToRow(new[]
            {
                y.Id.ToString(),
                y.DisplayName,
                lessonsCompleted.ToString(),
                present.ToString(),
                total.ToString(),
            }));
            sb.Append("\r\n");
        }

        await _audit.LogAsync("DataExported", _currentUser.UserId, "Cohort", request.CohortId,
            cancellationToken: cancellationToken);

        return new CsvFile($"cohort-{request.CohortId}-report.csv", sb.ToString());
    }
}
