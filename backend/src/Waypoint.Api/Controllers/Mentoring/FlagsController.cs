using Waypoint.Api.Contracts.Mentoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.Mentoring.Flags;

namespace Waypoint.Api.Controllers.Mentoring;

[ApiController]
[Route("api/flags")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class FlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlagsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveFlagRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResolveFlagCommand(id, request.ResolutionReason), cancellationToken);
        return NoContent();
    }
}
