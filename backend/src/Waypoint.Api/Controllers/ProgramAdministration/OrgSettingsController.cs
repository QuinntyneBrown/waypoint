using Waypoint.Api.Contracts.ProgramAdministration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Curriculum;
using Waypoint.Application.ProgramAdministration.Settings;

namespace Waypoint.Api.Controllers.ProgramAdministration;

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

    [HttpGet("tracks")]
    public async Task<ActionResult<IReadOnlyList<TrackSummary>>> Tracks(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTracksQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("terms")]
    public async Task<ActionResult<IReadOnlyList<TermView>>> Terms(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTermsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("terms")]
    public async Task<ActionResult<TermResult>> CreateTerm(
        [FromBody] SaveTermRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SaveTermCommand(
                null,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Status),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("terms/{termId:guid}")]
    public async Task<IActionResult> UpdateTerm(
        Guid termId,
        [FromBody] SaveTermRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SaveTermCommand(
                termId,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Status),
            cancellationToken);
        return NoContent();
    }
}
