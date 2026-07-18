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
}
