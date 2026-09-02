using Waypoint.Api.Contracts.PublicSite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.PublicSite;

namespace Waypoint.Api.Controllers.PublicSite;

/// <summary>
/// Public marketing site endpoints. Every action is explicitly anonymous (L2-PUB-001): the API sets no
/// fallback auth policy, and these endpoints must be reachable without a token and must not leak
/// authenticated data.
/// </summary>
[ApiController]
public class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpGet("api/public/content")]
    public async Task<ActionResult<PublicContentView>> Content(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPublicContentQuery(), cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("api/public/contact")]
    public async Task<ActionResult<SubmitContactRequestResult>> Contact(
        [FromBody] ContactRequestBody request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitContactRequestCommand(request.Name, request.Email, request.Topic, request.Message),
            cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("api/public/volunteer-signup")]
    public async Task<ActionResult<SubmitVolunteerSignupResult>> VolunteerSignup(
        [FromBody] VolunteerSignupBody request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitVolunteerSignupCommand(
                request.Name,
                request.Email,
                request.Phone,
                request.AreasOfInterest,
                request.ScreeningConsentAck),
            cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("api/public/donation")]
    public async Task<ActionResult<SubmitDonationPledgeResult>> Donation(
        [FromBody] DonationPledgeBody request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitDonationPledgeCommand(
                request.DonorName,
                request.Email,
                request.Tier,
                request.MonthlyAmount,
                request.Frequency,
                request.ProviderReference),
            cancellationToken);
        return Ok(result);
    }
}
