using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.LearningExperience.YouthCurriculum;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Policy = Policies.YouthParticipation)]
[Route("api/me/progress")]
public class YouthProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public YouthProgressController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ProgressView>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProgressQuery(), cancellationToken);
        return Ok(result);
    }
}
