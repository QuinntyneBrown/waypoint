using Waypoint.Api.Contracts.LearningExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Curriculum;
using Waypoint.Application.LearningExperience.YouthCurriculum;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Policy = Policies.YouthParticipation)]
[Route("api/me")]
public class YouthCurriculumController : ControllerBase
{
    private readonly IMediator _mediator;

    public YouthCurriculumController(IMediator mediator) => _mediator = mediator;

    [HttpGet("home")]
    public async Task<ActionResult<YouthHomeView>> Home(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new YouthHomeQuery(), cancellationToken));

    [HttpGet("curriculum")]
    public async Task<ActionResult<CurriculumView>> Browse(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new BrowseCurriculumQuery(), cancellationToken);
        return Ok(result);
    }

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

    [HttpGet("lessons/{id:guid}/media/{mediaId:guid}/playback")]
    public async Task<ActionResult<VideoPlaybackResult>> VideoPlayback(
        Guid id, Guid mediaId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new LessonVideoPlaybackQuery(mediaId, id), cancellationToken));

    [HttpGet("lessons/{id:guid}")]
    public async Task<ActionResult<LessonView>> Lesson(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LessonQuery(id), cancellationToken);
        return Ok(result);
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
