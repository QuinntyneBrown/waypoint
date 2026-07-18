using WordUp.Api.Contracts.Engagement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Engagement.Rsvps;

namespace WordUp.Api.Controllers.Engagement;

[ApiController]
public class RsvpController : ControllerBase
{
    private readonly IMediator _mediator;

    public RsvpController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/me/schedule")]
    [Authorize(Roles = Roles.Youth)]
    public async Task<ActionResult<IReadOnlyList<YouthSessionView>>> Schedule(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new YouthScheduleQuery(), cancellationToken));

    [HttpPost("api/sessions/{sessionId:guid}/rsvp")]
    [Authorize(Roles = Roles.Youth)]
    public async Task<ActionResult<RsvpResult>> Submit(
        Guid sessionId,
        [FromBody] SubmitRsvpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitRsvpCommand(sessionId, request.Response, request.Note, request.ReminderSet),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/sessions/{sessionId:guid}/rsvps")]
    [Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
    public async Task<ActionResult<IReadOnlyList<RsvpView>>> List(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SessionRsvpsQuery(sessionId), cancellationToken);
        return Ok(result);
    }
}
