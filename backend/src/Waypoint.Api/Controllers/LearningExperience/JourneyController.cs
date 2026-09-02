using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.LearningExperience.YouthCurriculum;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Policy = Policies.YouthParticipation)]
[Route("api/me/journey")]
public class JourneyController : ControllerBase
{
    private readonly IMediator _mediator;

    public JourneyController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<JourneyView>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new JourneyQuery(), cancellationToken);
        return Ok(result);
    }
}
