using WordUp.Api.Contracts.Mentoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Mentoring.Flags;
using WordUp.Application.Mentoring.MentorRoster;

namespace WordUp.Api.Controllers.Mentoring;

[ApiController]
[Route("api/mentor")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class MentorController : ControllerBase
{
    private readonly IMediator _mediator;

    public MentorController(IMediator mediator) => _mediator = mediator;

    [HttpGet("dashboard")]
    public async Task<ActionResult<MentorDashboard>> Dashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MentorDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("youth")]
    public async Task<ActionResult<IReadOnlyList<RosterYouth>>> Roster(
        [FromQuery] string? filter = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new RosterQuery(filter, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("youth/{id:guid}")]
    public async Task<ActionResult<YouthDetail>> YouthDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new YouthDetailQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reviews")]
    public async Task<ActionResult<IReadOnlyList<MentorReviewPreview>>> Reviews(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ReviewQueueQuery(take), cancellationToken);
        return Ok(result);
    }

    [HttpPost("youth/{id:guid}/flag")]
    public async Task<IActionResult> RaiseFlag(Guid id, [FromBody] RaiseFlagRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RaiseFlagCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpGet("flags")]
    public async Task<ActionResult<IReadOnlyList<AtRiskYouth>>> AtRisk(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AtRiskListQuery(), cancellationToken);
        return Ok(result);
    }
}
