using WordUp.Api.Contracts.ConsentAndSafeguarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.ConsentAndSafeguarding;

namespace WordUp.Api.Controllers.ConsentAndSafeguarding;

[ApiController]
[Route("api/consent")]
public class ConsentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConsentController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = Roles.Guardian)]
    [HttpPost]
    public async Task<ActionResult<ConsentResult>> Capture(
        [FromBody] CaptureConsentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CaptureConsentCommand(
                request.YouthUserId,
                request.GuardianName,
                request.Relationship,
                request.ParticipationConsent,
                request.MediaConsent,
                request.MediaAllPhotosVideo,
                request.MediaGroupOnly,
                request.MediaFirstNameCaptions,
                request.Signature,
                request.Term),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Guardian)]
    [HttpPost("{consentId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid consentId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new WithdrawConsentCommand(consentId), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.Youth)]
    [HttpGet("status")]
    public async Task<ActionResult<ConsentStatusResult>> Status(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsentStatusQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Youth)]
    [HttpPost("request")]
    public async Task<ActionResult<ConsentDeliveryResult>> RequestGuardianConsent(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestGuardianConsentCommand(), cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("/api/public/guardian-consent/{token}")]
    public async Task<ActionResult<RequestedConsentView>> RequestedConsent(
        string token,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestedConsentQuery(token), cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("/api/public/guardian-consent/{token}")]
    public async Task<ActionResult<ConsentResult>> CaptureRequestedConsent(
        string token,
        [FromBody] CaptureRequestedConsentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CaptureRequestedConsentCommand(
                token,
                request.GuardianName,
                request.Relationship,
                request.ParticipationConsent,
                request.MediaConsent,
                request.MediaAllPhotosVideo,
                request.MediaGroupOnly,
                request.MediaFirstNameCaptions,
                request.Signature),
            cancellationToken);
        return Ok(result);
    }
}
