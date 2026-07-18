using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.YouthCurriculum;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Youth)]
[Route("api/me")]
public class YouthCurriculumController : ControllerBase
{
    private readonly IMediator _mediator;

    public YouthCurriculumController(IMediator mediator) => _mediator = mediator;

    [HttpGet("curriculum/{trackId:guid}")]
    public async Task<ActionResult<CurriculumView>> Browse(Guid trackId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new BrowseCurriculumQuery(trackId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("lessons/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CompleteLessonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("lessons/{id:guid}/reading-state")]
    public async Task<IActionResult> SaveReadingState(
        Guid id, [FromBody] SaveReadingStateRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SaveReadingStateCommand(id, request.TextSize, request.Bookmarked, request.Position),
            cancellationToken);
        return NoContent();
    }
}

public record SaveReadingStateRequest(string? TextSize, bool Bookmarked, int Position);
