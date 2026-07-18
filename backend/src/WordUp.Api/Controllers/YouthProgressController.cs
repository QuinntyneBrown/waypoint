using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.YouthCurriculum;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Youth)]
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
