using WordUp.Api.Contracts.ProgramAdministration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.ProgramAdministration.Settings;

namespace WordUp.Api.Controllers.ProgramAdministration;

[ApiController]
[Route("api/org-settings")]
[Authorize(Policy = Policies.ManageUsers)]
public class OrgSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrgSettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<OrgSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrgSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateOrgSettingsRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateOrgSettingsCommand(
                request.Name,
                request.DisplayName,
                request.LogoUrl,
                request.City,
                request.ContactEmail,
                request.ConsentStatement,
                request.RequireReConsentEachTerm,
                request.RequireMediaConsent),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("tracks/{trackId:guid}/enabled")]
    public async Task<IActionResult> SetTrackEnabled(Guid trackId, [FromBody] SetTrackEnabledRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetTrackEnabledCommand(trackId, request.Enabled), cancellationToken);
        return NoContent();
    }
}
