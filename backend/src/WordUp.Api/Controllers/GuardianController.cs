using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Announcements;
using WordUp.Application.GuardianPortal;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Guardian)]
public class GuardianController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuardianController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/guardian/children/{childId:guid}/progress")]
    public async Task<ActionResult<ChildProgressView>> Progress(Guid childId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChildProgressQuery(childId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/guardian/announcements")]
    public async Task<ActionResult<IReadOnlyList<GuardianAnnouncementView>>> Announcements(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GuardianAnnouncementsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/guardian/announcements/read-all")]
    public async Task<IActionResult> MarkAllAnnouncementsRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllAnnouncementsReadCommand(), cancellationToken);
        return NoContent();
    }

    [HttpGet("api/guardian/children/{childId:guid}/consent")]
    public async Task<ActionResult<IReadOnlyList<GuardianConsentRecordView>>> Consent(
        Guid childId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GuardianConsentRecordQuery(childId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/guardian/children/{childId:guid}/media-consent")]
    public async Task<IActionResult> UpdateMediaConsent(
        Guid childId, [FromBody] UpdateMediaConsentRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateMediaConsentCommand(
                childId,
                request.MediaConsent,
                request.MediaAllPhotosVideo,
                request.MediaGroupOnly,
                request.MediaFirstNameCaptions),
            cancellationToken);
        return NoContent();
    }

    [HttpPut("api/guardian/contact")]
    public async Task<IActionResult> UpdateContact(
        [FromBody] UpdateGuardianContactRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateGuardianContactCommand(
                request.DisplayName,
                request.Email,
                request.Relationship,
                request.Phone,
                request.EmergencyContactName,
                request.EmergencyContactPhone),
            cancellationToken);
        return NoContent();
    }
}

public record UpdateMediaConsentRequest(
    bool MediaConsent,
    bool MediaAllPhotosVideo,
    bool MediaGroupOnly,
    bool MediaFirstNameCaptions);

public record UpdateGuardianContactRequest(
    string DisplayName,
    string Email,
    string? Relationship,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone);
