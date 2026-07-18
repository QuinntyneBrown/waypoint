using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.ReportingAndData.DataTransfer;
using WordUp.Application.ReportingAndData.Reports;

namespace WordUp.Api.Controllers.ReportingAndData;

[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/youth/{youthId:guid}")]
    public async Task<ActionResult<YouthReport>> Youth(Guid youthId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new YouthReportQuery(youthId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/cohort/{cohortId:guid}")]
    public async Task<ActionResult<CohortReport>> Cohort(Guid cohortId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CohortReportQuery(cohortId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator)]
    [HttpGet("api/reports/staff-activity")]
    public async Task<ActionResult<IReadOnlyList<StaffActivityRow>>> StaffActivity(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new StaffActivityReportQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/reports/cohort/{cohortId:guid}/export.csv")]
    public async Task<ActionResult<CsvFile>> ExportCohort(Guid cohortId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExportCohortReportQuery(cohortId), cancellationToken);
        return Ok(result);
    }
}
