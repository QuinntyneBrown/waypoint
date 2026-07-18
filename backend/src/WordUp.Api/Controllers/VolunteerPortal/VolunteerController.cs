using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.VolunteerPortal;

namespace WordUp.Api.Controllers.VolunteerPortal;

[ApiController]
[Route("api/volunteer")]
[Authorize(Roles = Roles.Volunteer + "," + Roles.ProgramAdministrator)]
public class VolunteerController : ControllerBase
{
    private readonly IMediator _mediator;

    public VolunteerController(IMediator mediator) => _mediator = mediator;

    [HttpGet("today")]
    public async Task<ActionResult<VolunteerTodayView?>> Today(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VolunteerTodayQuery(), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpGet("sessions/{sessionId:guid}/roster")]
    public async Task<ActionResult<IReadOnlyList<VolunteerRosterEntry>>> Roster(
        Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VolunteerRosterQuery(sessionId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<IReadOnlyList<VolunteerScheduleItem>>> Schedule(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VolunteerScheduleQuery(), cancellationToken);
        return Ok(result);
    }
}
