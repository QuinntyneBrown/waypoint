using Waypoint.Api.Contracts.Engagement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.Engagement.Sessions;

namespace Waypoint.Api.Controllers.Engagement;

[ApiController]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/sessions/options")]
    public async Task<ActionResult<SessionSchedulingOptions>> Options(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new SessionOptionsQuery(), cancellationToken));

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpPost("api/sessions")]
    public async Task<ActionResult<CreateSessionResult>> Create(
        [FromBody] CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateSessionCommand(
                request.CohortId,
                request.MentorId,
                request.Title,
                request.StartsAt,
                request.DurationMinutes,
                request.Location,
                request.RepeatsWeekly),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    [HttpGet("api/sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionView>>> Calendar(
        [FromQuery] Guid? cohortId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SessionCalendarQuery(cohortId, from, to), cancellationToken);
        return Ok(result);
    }
}
