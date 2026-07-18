using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Grading;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/mentor")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class GradingController : ControllerBase
{
    private readonly IMediator _mediator;

    public GradingController(IMediator mediator) => _mediator = mediator;

    // Grade a youth submission (L2-MENTOR-003).
    [HttpPost("submissions/{id:guid}/grade")]
    public async Task<IActionResult> Grade(
        Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new GradeSubmissionCommand(
                id,
                request.Decision,
                request.RubricScoresJson,
                request.TotalAwarded,
                request.Comment,
                request.GuardianNotified),
            cancellationToken);
        return NoContent();
    }
}

public record GradeSubmissionRequest(
    string Decision,
    string? RubricScoresJson,
    int? TotalAwarded,
    string? Comment,
    bool GuardianNotified);
