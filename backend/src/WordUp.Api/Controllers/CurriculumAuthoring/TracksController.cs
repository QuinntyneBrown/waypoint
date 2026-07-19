using WordUp.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.CurriculumAuthoring.Curriculum;

namespace WordUp.Api.Controllers.CurriculumAuthoring;

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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackWorkspaceView>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTrackWorkspaceQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id,
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishTrackCommand(id, request.Status), cancellationToken);
        return NoContent();
    }
}
