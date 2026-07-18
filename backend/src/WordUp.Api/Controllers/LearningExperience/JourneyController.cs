using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.LearningExperience.YouthCurriculum;

namespace WordUp.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Roles = Roles.Youth)]
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
