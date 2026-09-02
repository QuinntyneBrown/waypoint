using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.PlatformServices.Access;

namespace Waypoint.Api.Controllers.PlatformServices;

[ApiController]
public class AccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccessController(IMediator mediator) => _mediator = mediator;

    // L2-PLATFORM-011: youth participation gate (cohort placement + guardian consent).
    [HttpGet("api/me/access")]
    [Authorize(Roles = Roles.Youth)]
    public async Task<ActionResult<AccessState>> Access(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AccessStateQuery(), cancellationToken);
        return Ok(result);
    }
}
