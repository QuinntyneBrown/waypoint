using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.QuizApproval;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/mentor")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class QuizApprovalController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizApprovalController(IMediator mediator) => _mediator = mediator;

    // Approve a single quiz attempt, optionally overriding the auto score (L2-MENTOR-004, L2-LEARN-005).
    [HttpPost("attempts/{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApproveQuizAttemptRequest? request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveQuizAttemptCommand(id, request?.OverrideScore), cancellationToken);
        return NoContent();
    }

    // Approve all pending attempts for the mentor's assigned youth (L2-MENTOR-004 AC3).
    [HttpPost("attempts/approve-all")]
    public async Task<ActionResult<int>> ApproveAll(CancellationToken cancellationToken)
    {
        var count = await _mediator.Send(new ApproveAllQuizAttemptsCommand(), cancellationToken);
        return Ok(count);
    }
}

public record ApproveQuizAttemptRequest(int? OverrideScore);
