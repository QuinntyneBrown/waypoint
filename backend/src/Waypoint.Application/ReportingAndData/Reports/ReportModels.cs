namespace Waypoint.Application.ReportingAndData.Reports;

public record ReportFilter(
    DateTime? From = null,
    DateTime? To = null,
    Guid? CohortId = null,
    Guid? TrackId = null,
    Guid? MentorId = null);

public record ReportOption(Guid Id, string Name);

public record ReportOptions(
    IReadOnlyList<ReportOption> Cohorts,
    IReadOnlyList<ReportOption> Tracks,
    IReadOnlyList<ReportOption> Mentors,
    IReadOnlyList<ReportOption> Youth);

public record AttendanceBreakdown(
    int Present,
    int Late,
    int Excused,
    int Absent,
    int Total,
    double Rate);

public record ModuleReport(
    Guid ModuleId,
    string Name,
    int CompletedLessons,
    int TotalLessons,
    double CompletionRate);

public record CohortMetric(
    Guid CohortId,
    string CohortName,
    int Enrolment,
    double CompletionRate,
    double AttendanceRate,
    double RetentionRate,
    int AtRisk);

public record ReportKpis(
    int ActiveYouth,
    int Cohorts,
    double AvgCompletion,
    double AttendanceRate,
    double RetentionRate,
    int ActiveMentors);
