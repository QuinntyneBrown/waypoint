using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Curriculum;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize(Policy = Policies.EditCurriculum)]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<LessonResult>> Create(
        [FromBody] CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateLessonCommand(
                request.ModuleId,
                request.Title,
                request.ScriptureReference,
                request.EstimatedMinutes,
                request.LearningObjectives,
                request.TeachingBody,
                request.ReflectionPrompts,
                request.LeaderGuideNotes),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LessonDetail>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id, [FromBody] PublishRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishLessonCommand(id, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLessonCommand(id), cancellationToken);
        return NoContent();
    }
}

public record CreateLessonRequest(
    Guid ModuleId,
    string Title,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes);
