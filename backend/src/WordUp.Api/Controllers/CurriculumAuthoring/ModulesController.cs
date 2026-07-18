using WordUp.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.CurriculumAuthoring.Curriculum;

namespace WordUp.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/modules")]
[Authorize(Policy = Policies.EditCurriculum)]
public class ModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModulesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ModuleResult>> Create(
        [FromBody] CreateModuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateModuleCommand(
                request.TrackId, request.Name, request.Description, request.PrerequisiteModuleId, request.ScriptureAnchor),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderModulesRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReorderModulesCommand(request.TrackId, request.OrderedModuleIds), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id, [FromBody] PublishRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishModuleCommand(id, request.Status), cancellationToken);
        return NoContent();
    }
}
