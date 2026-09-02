using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.ConsentAndSafeguarding;

namespace Waypoint.Api.Controllers.ConsentAndSafeguarding;

[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.ViewAuditLog)]
    [HttpGet]
    public async Task<ActionResult<AuditLogResult>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null,
        [FromQuery] Guid? actorUserId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AuditLogQuery(page, pageSize, action, actorUserId, from, to),
            cancellationToken);
        return Ok(result);
    }
}
