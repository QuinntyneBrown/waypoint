using Waypoint.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Curriculum;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateLessonRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateLessonCommand(
                id,
                request.ModuleId,
                request.Title,
                request.ScriptureReference,
                request.EstimatedMinutes,
                request.LearningObjectives,
                request.TeachingBody,
                request.ReflectionPrompts,
                request.LeaderGuideNotes),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderLessonsRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ReorderLessonsCommand(request.ModuleId, request.OrderedLessonIds),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/media")]
    [RequestSizeLimit(LessonMediaUploadPolicy.MaxBytes + 65536)]
    public async Task<ActionResult<LessonMediaResult>> AddMedia(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var result = await _mediator.Send(
            new StoreLessonMediaCommand(
                id,
                file.FileName,
                file.ContentType.ToLowerInvariant(),
                stream.ToArray()),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/media/{mediaId:guid}/retry-transcode")]
    public async Task<IActionResult> RetryTranscode(
        Guid lessonId,
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RetryVideoTranscodeCommand(lessonId, mediaId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{lessonId:guid}/media/{mediaId:guid}")]
    public async Task<IActionResult> RemoveMedia(
        Guid lessonId,
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLessonMediaCommand(lessonId, mediaId), cancellationToken);
        return NoContent();
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
