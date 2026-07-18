using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Attendance;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.RecordAttendance)]
    [HttpPost("api/attendance")]
    public async Task<ActionResult<AttendanceTally>> Save(
        [FromBody] SaveAttendanceRequest request, CancellationToken cancellationToken)
    {
        var marks = (request.Marks ?? new List<AttendanceMarkInput>())
            .Select(m => new AttendanceMark(m.YouthId, m.Status, m.Note, m.ExpectedVersion))
            .ToList();
        var result = await _mediator.Send(new SaveAttendanceCommand(request.SessionId, marks), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor + "," + Roles.Volunteer)]
    [HttpGet("api/attendance/{sessionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceMarkView>>> Session(
        Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SessionAttendanceQuery(sessionId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor + "," + Roles.Volunteer)]
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

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor + "," + Roles.Volunteer)]
    [HttpGet("api/attendance/history/by-youth")]
    public async Task<ActionResult<YouthAttendanceSummary>> HistoryByYouth(
        [FromQuery] Guid youthId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AttendanceHistoryByYouthQuery(youthId), cancellationToken);
        return Ok(result);
    }
}

public record SaveAttendanceRequest(Guid SessionId, List<AttendanceMarkInput>? Marks);

public record AttendanceMarkInput(Guid YouthId, string Status, string? Note, string? ExpectedVersion = null);
