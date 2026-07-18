using WordUp.Api.Contracts.Engagement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Engagement.Attendance;

namespace WordUp.Api.Controllers.Engagement;

[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.RecordAttendance)]
    [HttpPost("api/attendance")]
    public async Task<ActionResult<AttendanceTally>> Save(
        [FromBody] SaveAttendanceRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var marks = (request.Marks ?? new List<AttendanceMarkInput>())
            .Select(m => new AttendanceMark(m.YouthId, m.Status, m.Note, m.ExpectedVersion))
            .ToList();
        var result = await _mediator.Send(
            new SaveAttendanceCommand(request.SessionId, marks, idempotencyKey),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/{sessionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceMarkView>>> Session(
        Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SessionAttendanceQuery(sessionId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/history/by-session")]
    public async Task<ActionResult<IReadOnlyList<SessionAttendanceSummary>>> HistoryBySession(
        [FromQuery] Guid? cohortId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AttendanceHistoryBySessionQuery(cohortId, from, to), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/history/by-youth")]
    public async Task<ActionResult<YouthAttendanceSummary>> HistoryByYouth(
        [FromQuery] Guid youthId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AttendanceHistoryByYouthQuery(youthId, from, to), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/history/youth")]
    public async Task<ActionResult<IReadOnlyList<YouthAttendanceSummary>>> HistoryYouthList(
        [FromQuery] Guid? cohortId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AttendanceHistoryYouthListQuery(cohortId, from, to), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/{sessionId:guid}/roster")]
    public async Task<ActionResult<StaffSessionAttendance>> StaffRoster(
        Guid sessionId,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new StaffSessionAttendanceQuery(sessionId), cancellationToken));

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/attendance/history/export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format,
        [FromQuery] string view,
        [FromQuery] Guid? cohortId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AttendanceExportQuery(format, view, cohortId, from, to), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
