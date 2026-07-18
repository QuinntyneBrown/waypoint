using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Announcements;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnnouncementsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/announcements")]
    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    public async Task<ActionResult<AnnouncementResult>> Compose(
        [FromBody] ComposeAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ComposeAnnouncementCommand(
                request.Title,
                request.Message,
                request.AudienceScope,
                request.CohortId,
                request.RoleTargeting,
                request.ScheduledAt),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/announcements")]
    public async Task<ActionResult<IReadOnlyList<AnnouncementView>>> Mine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MyAnnouncementsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/announcements/{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAnnouncementReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("api/announcements/read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllAnnouncementsReadCommand(), cancellationToken);
        return NoContent();
    }

    [HttpGet("api/announcements/{id:guid}/stats")]
    [Authorize(Roles = Roles.ProgramAdministrator + "," + Roles.Mentor)]
    public async Task<ActionResult<AnnouncementStats>> Stats(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AnnouncementStatsQuery(id), cancellationToken);
        return Ok(result);
    }
}

public record ComposeAnnouncementRequest(
    string Title,
    string Message,
    string AudienceScope,
    Guid? CohortId,
    string RoleTargeting,
    DateTime? ScheduledAt);
