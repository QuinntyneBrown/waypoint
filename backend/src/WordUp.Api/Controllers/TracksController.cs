using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Curriculum;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/tracks")]
[Authorize(Policy = Policies.EditCurriculum)]
public class TracksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TracksController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<TrackResult>> Create(
        [FromBody] CreateTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateTrackCommand(request.Name, request.Key, request.Description), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackSummary>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTracksQuery(), cancellationToken);
        return Ok(result);
    }
}

public record CreateTrackRequest(string Name, string Key, string? Description);
