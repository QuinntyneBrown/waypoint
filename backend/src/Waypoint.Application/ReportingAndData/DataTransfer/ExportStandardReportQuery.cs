using System.Globalization;
using System.Text;
using FluentValidation;
using MediatR;
using Waypoint.Application.Engagement.Attendance;
using Waypoint.Application.ReportingAndData.Reports;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ReportingAndData.DataTransfer;

public sealed record ExportStandardReportQuery(
    string Report,
    string Format,
    ReportFilter Filter,
    Guid? YouthId = null,
    Guid? CohortId = null,
    Guid? CompareCohortId = null) : IRequest<DataExportFile>;

public sealed record DataExportFile(string FileName, string ContentType, byte[] Content);

public sealed class ExportStandardReportQueryHandler
    : IRequestHandler<ExportStandardReportQuery, DataExportFile>
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;

    public ExportStandardReportQueryHandler(
        ISender sender,
        ICurrentUser currentUser,
        IAuditLogger audit,
        IClock clock)
    {
        _sender = sender;
        _currentUser = currentUser;
        _audit = audit;
        _clock = clock;
    }

    public async Task<DataExportFile> Handle(
        ExportStandardReportQuery request,
        CancellationToken cancellationToken)
    {
        var report = request.Report.Trim().ToLowerInvariant();
        var format = request.Format.Trim().ToLowerInvariant();
        if (report is not ("overview" or "youth" or "cohort" or "staff"))
            throw new ValidationException("Report must be overview, youth, cohort, or staff.");
        if (format is not ("csv" or "pdf"))
            throw new ValidationException("Export format must be csv or pdf.");
        if (report == "staff" && _currentUser.Role != Roles.ProgramAdministrator)
            throw new ForbiddenException("Staff activity exports are limited to Program Administrators.");

        var (title, rows, targetId) = report switch
        {
            "overview" => await OverviewAsync(request.Filter, cancellationToken),
            "youth" => await YouthAsync(request, cancellationToken),
            "cohort" => await CohortAsync(request, cancellationToken),
            "staff" => await StaffAsync(request.Filter, cancellationToken),
            _ => throw new InvalidOperationException(),
        };
        var stamp = _clock.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var file = format == "csv"
            ? new DataExportFile(
                $"{report}-report-{stamp}.csv",
                "text/csv; charset=utf-8",
                Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, rows) + Environment.NewLine))
            : new DataExportFile(
                $"{report}-report-{stamp}.pdf",
                "application/pdf",
                SimplePdf.Create(title, rows));

        await _audit.LogAsync(
            "DataExported",
            _currentUser.UserId,
            "Report",
            targetId,
            details: AuditDetails(report, format, request),
            cancellationToken: cancellationToken);
        return file;
    }

    private async Task<(string Title, IReadOnlyList<string> Rows, Guid? TargetId)> OverviewAsync(
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var hub = await _sender.Send(new ReportsHubQuery(filter), cancellationToken);
        var rows = new List<string>
        {
            Csv.ToRow(["Report", "Organization overview"]),
            Csv.ToRow(["Generated at", hub.GeneratedAt.ToString("o", CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Youth records", hub.RecordCount.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["KPI", "Value"]),
            Csv.ToRow(["Active youth", hub.Kpis.ActiveYouth.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Cohorts", hub.Kpis.Cohorts.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Average completion", Percent(hub.Kpis.AvgCompletion)]),
            Csv.ToRow(["Attendance", Percent(hub.Kpis.AttendanceRate)]),
            Csv.ToRow(["Retention", Percent(hub.Kpis.RetentionRate)]),
            Csv.ToRow(["Active mentors", hub.Kpis.ActiveMentors.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Attendance status", "Count"]),
            Csv.ToRow(["Present", hub.Attendance.Present.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Late", hub.Attendance.Late.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Excused", hub.Attendance.Excused.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Absent", hub.Attendance.Absent.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Cohort", "Enrollment", "Completion", "Attendance", "Retention", "At risk"]),
        };
        rows.AddRange(hub.Cohorts.Select(item => Csv.ToRow([
            item.CohortName,
            item.Enrolment.ToString(CultureInfo.InvariantCulture),
            Percent(item.CompletionRate),
            Percent(item.AttendanceRate),
            Percent(item.RetentionRate),
            item.AtRisk.ToString(CultureInfo.InvariantCulture),
        ])));
        rows.Add(Csv.ToRow(["Mentor", "Email", "Caseload", "Check-ins", "Reviews", "Load balance"]));
        rows.AddRange(hub.Staff.Select(item => Csv.ToRow([
            item.MentorName,
            item.Email,
            item.Caseload.ToString(CultureInfo.InvariantCulture),
            item.CheckIns.ToString(CultureInfo.InvariantCulture),
            item.Reviews.ToString(CultureInfo.InvariantCulture),
            item.LoadBalance,
        ])));
        return ("Waypoint organization report", rows, null);
    }

    private async Task<(string Title, IReadOnlyList<string> Rows, Guid? TargetId)> YouthAsync(
        ExportStandardReportQuery request,
        CancellationToken cancellationToken)
    {
        if (request.YouthId is not Guid youthId)
            throw new ValidationException("A youth is required for a youth report export.");
        var report = await _sender.Send(new YouthReportQuery(youthId, request.Filter), cancellationToken);
        var rows = new List<string>
        {
            Csv.ToRow(["Report", "Youth progress"]),
            Csv.ToRow(["Youth", report.YouthName]),
            Csv.ToRow(["Cohort", report.CohortName ?? string.Empty]),
            Csv.ToRow(["Mentor", report.MentorName ?? string.Empty]),
            Csv.ToRow(["Lessons completed", report.LessonsCompleted.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Lessons total", report.LessonsTotal.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Completion", Percent(report.CompletionRate)]),
            Csv.ToRow(["Quiz average", report.AvgQuizScore.ToString("0.#", CultureInfo.InvariantCulture) + "%"]),
            Csv.ToRow(["Attendance", Percent(report.Attendance.Rate)]),
            Csv.ToRow(["Mentor check-ins", report.MentorNotes.CheckIns.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Module", "Completed lessons", "Total lessons", "Completion"]),
        };
        rows.AddRange(report.Modules.Select(item => Csv.ToRow([
            item.Name,
            item.CompletedLessons.ToString(CultureInfo.InvariantCulture),
            item.TotalLessons.ToString(CultureInfo.InvariantCulture),
            Percent(item.CompletionRate),
        ])));
        rows.Add(Csv.ToRow(["Quiz", "Score", "Submitted"]));
        rows.AddRange(report.QuizScores.Select(item => Csv.ToRow([
            item.Title,
            item.Score.ToString(CultureInfo.InvariantCulture) + "%",
            item.SubmittedAt?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
        ])));
        rows.Add(Csv.ToRow(["Competency", "Level", "Assessed"]));
        rows.AddRange(report.Competencies.Select(item => Csv.ToRow([
            item.Name,
            item.Level,
            item.AssessedAt.ToString("o", CultureInfo.InvariantCulture),
        ])));
        rows.Add(Csv.ToRow(["Badge", "Awarded"]));
        rows.AddRange(report.BadgeAwards.Select(item => Csv.ToRow([
            item.Name,
            item.AwardedAt.ToString("o", CultureInfo.InvariantCulture),
        ])));
        return ($"Waypoint youth report — {report.YouthName}", rows, youthId);
    }

    private async Task<(string Title, IReadOnlyList<string> Rows, Guid? TargetId)> CohortAsync(
        ExportStandardReportQuery request,
        CancellationToken cancellationToken)
    {
        var cohortId = request.CohortId ?? request.Filter.CohortId;
        if (cohortId is not Guid selectedCohortId)
            throw new ValidationException("A cohort is required for a cohort report export.");
        var filter = request.Filter with { CohortId = selectedCohortId };
        var report = await _sender.Send(
            new CohortReportQuery(selectedCohortId, filter, request.CompareCohortId),
            cancellationToken);
        var rows = new List<string>
        {
            Csv.ToRow(["Report", "Cohort"]),
            Csv.ToRow(["Cohort", report.CohortName]),
            Csv.ToRow(["Enrollment", report.Enrolment.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Completion", Percent(report.AvgCompletion)]),
            Csv.ToRow(["Attendance", Percent(report.AttendanceRate)]),
            Csv.ToRow(["Retention", Percent(report.RetentionRate)]),
            Csv.ToRow(["Active", report.StatusActive.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["At risk", report.StatusAtRisk.ToString(CultureInfo.InvariantCulture)]),
            Csv.ToRow(["Module", "Completed lessons", "Total lessons", "Completion"]),
        };
        rows.AddRange(report.Modules.Select(item => Csv.ToRow([
            item.Name,
            item.CompletedLessons.ToString(CultureInfo.InvariantCulture),
            item.TotalLessons.ToString(CultureInfo.InvariantCulture),
            Percent(item.CompletionRate),
        ])));
        rows.Add(Csv.ToRow(["Youth", "Completion", "Attendance", "Status"]));
        rows.AddRange(report.Youth.Select(item => Csv.ToRow([
            item.YouthName,
            Percent(item.CompletionRate),
            Percent(item.AttendanceRate),
            item.Status,
        ])));
        rows.Add(Csv.ToRow(["Comparison", "Completion", "Attendance", "Retention"]));
        rows.AddRange(report.Comparisons.Select(item => Csv.ToRow([
            item.Label,
            Percent(item.CompletionRate),
            Percent(item.AttendanceRate),
            Percent(item.RetentionRate),
        ])));
        return ($"Waypoint cohort report — {report.CohortName}", rows, selectedCohortId);
    }

    private async Task<(string Title, IReadOnlyList<string> Rows, Guid? TargetId)> StaffAsync(
        ReportFilter filter,
        CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new StaffActivityReportQuery(filter), cancellationToken);
        var rows = new List<string>
        {
            Csv.ToRow(["Mentor", "Email", "Caseload", "Check-ins", "Reviews", "Target", "Load balance"]),
        };
        rows.AddRange(report.Select(item => Csv.ToRow([
            item.MentorName,
            item.Email,
            item.Caseload.ToString(CultureInfo.InvariantCulture),
            item.CheckIns.ToString(CultureInfo.InvariantCulture),
            item.Reviews.ToString(CultureInfo.InvariantCulture),
            $"{item.TargetMinimum}–{item.TargetMaximum}",
            item.LoadBalance,
        ])));
        return ("Waypoint staff activity report", rows, null);
    }

    private static string Percent(double value) => value.ToString("P0", CultureInfo.InvariantCulture);

    private static string AuditDetails(string report, string format, ExportStandardReportQuery request) =>
        string.Join(";", new[]
        {
            $"report={report}",
            $"format={format}",
            $"from={request.Filter.From:yyyy-MM-dd}",
            $"to={request.Filter.To:yyyy-MM-dd}",
            $"cohortId={request.CohortId ?? request.Filter.CohortId}",
            $"trackId={request.Filter.TrackId}",
            $"mentorId={request.Filter.MentorId}",
            $"youthId={request.YouthId}",
            $"compareCohortId={request.CompareCohortId}",
        });
}
