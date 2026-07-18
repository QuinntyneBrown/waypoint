using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Submissions;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Roles = Roles.Youth)]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator) => _mediator = mediator;

    // Submit assignment work (L2-LEARN-006); coding projects use IsCodingProject=true (L2-LEARN-008 AC2).
    [HttpPost("assignments/{assignmentId:guid}/submissions")]
    public async Task<ActionResult<SubmissionResult>> Submit(
        Guid assignmentId, [FromBody] SubmitAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitAssignmentCommand(
                assignmentId,
                request.BodyText,
                request.PhotoUrl,
                request.LinkUrl,
                request.IsCodingProject,
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
            new ResubmitCommand(id, request.BodyText, request.PhotoUrl, request.LinkUrl), cancellationToken);
        return NoContent();
    }
}

public record SubmitAssignmentRequest(
    string? BodyText,
    string? PhotoUrl,
    string? LinkUrl,
    bool IsCodingProject,
    bool Attest);

public record ResubmitRequest(string? BodyText, string? PhotoUrl, string? LinkUrl);
