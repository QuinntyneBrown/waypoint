using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Api.Contracts.CurriculumAuthoring;
using Waypoint.Application.CurriculumAuthoring.Curriculum;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/lessons/{lessonId:guid}/video-uploads")]
[Authorize(Policy = Policies.EditCurriculum)]
public sealed class LessonVideoUploadsController : ControllerBase
{
    // The largest configurable chunk (16 MiB) plus request overhead.
    private const long MaxChunkRequestBytes = 16L * 1024 * 1024 + 65536;

    private readonly IMediator _mediator;

    public LessonVideoUploadsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<VideoUploadSessionResult>> Start(
        Guid lessonId, [FromBody] StartVideoUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StartVideoUploadCommand(lessonId, request.FileName, request.SizeBytes),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{uploadId:guid}/chunks/{index:int}")]
    [RequestSizeLimit(MaxChunkRequestBytes)]
    public async Task<IActionResult> PutChunk(
        Guid lessonId, Guid uploadId, int index, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ReceiveVideoChunkCommand(
                lessonId, uploadId, index, Request.ContentLength, Request.Body),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{uploadId:guid}/complete")]
    public async Task<ActionResult<LessonMediaResult>> Complete(
        Guid lessonId, Guid uploadId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CompleteVideoUploadCommand(lessonId, uploadId), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{uploadId:guid}")]
    public async Task<ActionResult<VideoUploadStatusResult>> Status(
        Guid lessonId, Guid uploadId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new VideoUploadStatusQuery(lessonId, uploadId), cancellationToken));
}
