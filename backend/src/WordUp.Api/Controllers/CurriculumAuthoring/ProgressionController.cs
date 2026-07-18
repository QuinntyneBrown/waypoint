using WordUp.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.CurriculumAuthoring.Progression;

namespace WordUp.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/progression")]
public class ProgressionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgressionController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages")]
    public async Task<ActionResult<StageResult>> CreateStage([FromBody] CreateStageRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStageCommand(request.Name, request.Description, request.Order), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages/{stageId:guid}/competencies")]
    public async Task<ActionResult<CompetencyResult>> AddCompetency(Guid stageId, [FromBody] AddCompetencyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddCompetencyCommand(stageId, request.Name), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("stages/reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStagesRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReorderStagesCommand(request.OrderedStageIds), cancellationToken);
        return NoContent();
    }
}
