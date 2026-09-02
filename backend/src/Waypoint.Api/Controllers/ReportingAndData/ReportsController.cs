using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.ReportingAndData.DataTransfer;
using Waypoint.Application.ReportingAndData.Reports;

namespace Waypoint.Api.Controllers.ReportingAndData;

[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/youth/{youthId:guid}")]
    public async Task<ActionResult<YouthReport>> Youth(
        Guid youthId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? trackId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new YouthReportQuery(youthId, new ReportFilter(from, to, TrackId: trackId)),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/cohort/{cohortId:guid}")]
    public async Task<ActionResult<CohortReport>> Cohort(
        Guid cohortId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? trackId,
        [FromQuery] Guid? compareCohortId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CohortReportQuery(
                cohortId,
                new ReportFilter(from, to, cohortId, trackId),
                compareCohortId),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator)]
    [HttpGet("api/reports/staff-activity")]
    public async Task<ActionResult<IReadOnlyList<StaffActivityRow>>> StaffActivity(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? cohortId,
        [FromQuery] Guid? trackId,
        [FromQuery] Guid? mentorId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StaffActivityReportQuery(new ReportFilter(from, to, cohortId, trackId, mentorId)),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/options")]
    public async Task<ActionResult<ReportOptions>> Options(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new ReportOptionsQuery(), cancellationToken));
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/hub")]
    public async Task<ActionResult<ReportsHub>> Hub(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? cohortId,
        [FromQuery] Guid? trackId,
        [FromQuery] Guid? mentorId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new ReportsHubQuery(new ReportFilter(from, to, cohortId, trackId, mentorId)),
            cancellationToken));
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/export")]
    public async Task<IActionResult> Export(
        [FromQuery] string report,
        [FromQuery] string format,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? cohortId,
        [FromQuery] Guid? trackId,
        [FromQuery] Guid? mentorId,
        [FromQuery] Guid? youthId,
        [FromQuery] Guid? compareCohortId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ExportStandardReportQuery(
                report,
                format,
                new ReportFilter(from, to, cohortId, trackId, mentorId),
                youthId,
                cohortId,
                compareCohortId),
            cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/cohort/{cohortId:guid}/export.csv")]
    public async Task<ActionResult<CsvFile>> ExportCohort(Guid cohortId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExportCohortReportQuery(cohortId), cancellationToken);
        return Ok(result);
    }
}
