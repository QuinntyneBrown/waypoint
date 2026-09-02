using Waypoint.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Progression;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/progression")]
public class ProgressionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgressionController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgressionStageView>>> Get(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetProgressionQuery(), cancellationToken));

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages")]
    public async Task<ActionResult<StageResult>> CreateStage([FromBody] CreateStageRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStageCommand(request.Name, request.Description, request.Order), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPut("stages/{stageId:guid}")]
    public async Task<IActionResult> UpdateStage(
        Guid stageId,
        [FromBody] UpdateProgressionStageRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateStageCommand(stageId, request.Name, request.Description),
            cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages/{stageId:guid}/competencies")]
    public async Task<ActionResult<CompetencyResult>> AddCompetency(Guid stageId, [FromBody] AddCompetencyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddCompetencyCommand(stageId, request.Name), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpDelete("competencies/{competencyId:guid}")]
    public async Task<IActionResult> DeleteCompetency(Guid competencyId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCompetencyCommand(competencyId), cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages/reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStagesRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReorderStagesCommand(request.OrderedStageIds), cancellationToken);
        return NoContent();
    }
}
