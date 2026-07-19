using WordUp.Api.Contracts.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Authentication.Onboarding;

namespace WordUp.Api.Controllers.Authentication;

[ApiController]
[Authorize(Roles = Roles.Youth)]
[Route("api/onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<OnboardingStatusResult>> Status(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new OnboardingStatusQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] CompleteOnboardingRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CompleteOnboardingCommand(request.DisplayName, request.Pronouns), cancellationToken);
        return NoContent();
    }

    [HttpPost("skip")]
    public async Task<IActionResult> Skip(CancellationToken cancellationToken)
    {
        await _mediator.Send(new SkipOnboardingCommand(), cancellationToken);
        return NoContent();
    }
}
