using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Consent;

namespace WordUp.Api.Controllers;

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
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new AuditLogQuery(page, pageSize, action, actorUserId), cancellationToken);
        return Ok(result);
    }
}
