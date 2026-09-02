using Waypoint.Api.Contracts.LearningExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.LearningExperience.Submissions;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Route("api/me")]
[Authorize(Policy = Policies.YouthParticipation)]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("assignments/{assignmentId:guid}")]
    public async Task<ActionResult<AssignmentBriefView>> Brief(
        Guid assignmentId,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new AssignmentBriefQuery(assignmentId), cancellationToken));

    [HttpPost("assignments/{assignmentId:guid}/uploads")]
    [RequestSizeLimit(AssignmentUploadPolicy.MaxBytes + 65536)]
    public async Task<ActionResult<AssignmentUploadResult>> Upload(
        Guid assignmentId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return Ok(await _mediator.Send(
            new StoreAssignmentUploadCommand(
                assignmentId,
                file.FileName,
                file.ContentType.ToLowerInvariant(),
                stream.ToArray()),
            cancellationToken));
    }

    [HttpGet("files/{fileId:guid}")]
    public async Task<IActionResult> File(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new SubmissionFileQuery(fileId), cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return new FileContentResult(file.Content, file.ContentType)
        {
            FileDownloadName = file.FileName,
        };
    }

    // Submit assignment work (L2-LEARN-006); coding projects use IsCodingProject=true (L2-LEARN-008 AC2).
    [HttpPost("assignments/{assignmentId:guid}/submissions")]
    public async Task<ActionResult<SubmissionResult>> Submit(
        Guid assignmentId, [FromBody] SubmitAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitAssignmentCommand(
                assignmentId,
                request.BodyText,
                request.PhotoUploadId,
                request.FileUploadId,
                request.LinkUrl,
                request.IsCodingProject,
                request.YouthNote,
                request.Attest),
            cancellationToken);
        return Ok(result);
    }

    // Check submission status (L2-LEARN-007).
    [HttpGet("submissions/{id:guid}")]
    public async Task<ActionResult<SubmissionStatusView>> Status(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmissionStatusQuery(id), cancellationToken);
        return Ok(result);
    }

    // Resubmit after a mentor requested revisions (L2-LEARN-007 AC3).
    [HttpPost("submissions/{id:guid}/resubmit")]
    public async Task<IActionResult> Resubmit(
        Guid id, [FromBody] ResubmitRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResubmitCommand(
                id,
                request.BodyText,
                request.PhotoUrl,
                request.LinkUrl,
                request.ExpectedVersion),
            cancellationToken);
        return NoContent();
    }
}
