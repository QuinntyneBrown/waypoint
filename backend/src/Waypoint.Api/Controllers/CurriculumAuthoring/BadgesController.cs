using Waypoint.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Badges;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/badges")]
public class BadgesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BadgesController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BadgeDefinitionView>>> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListBadgesQuery(), cancellationToken));

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost]
    public async Task<ActionResult<BadgeResult>> Create([FromBody] CreateBadgeRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateBadgeCommand(
                request.Name, request.Icon, request.Description,
                request.CriteriaKind, request.AutoRule, request.TargetRefId, request.CompetencyId),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
    [HttpPost("{id:guid}/award")]
    public async Task<IActionResult> Award(Guid id, [FromBody] AwardBadgeRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AwardBadgeCommand(id, request.YouthUserId), cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBadgeCommand(id), cancellationToken);
        return NoContent();
    }
}
