using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.LearningExperience.YouthCurriculum;

namespace WordUp.Api.Controllers.LearningExperience;

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
