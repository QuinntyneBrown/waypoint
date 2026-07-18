using WordUp.Api.Contracts.ApplicationIntake;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.ApplicationIntake;

namespace WordUp.Api.Controllers.ApplicationIntake;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<SubmitApplicationResult>> Submit(
        [FromBody] SubmitApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitApplicationCommand(
            request.FirstName,
            request.LastName,
            request.Age,
            request.Neighbourhood,
            request.WhyInterested,
            request.TrackInterest,
            request.GuardianName,
            request.GuardianEmail,
            request.GuardianPhone,
            request.ParticipationConsentAck,
            request.PrivacyAck), cancellationToken);

        return CreatedAtAction(nameof(Submit), new { id = result.ApplicationId }, result);
    }

    [Authorize(Policy = Policies.ApproveApplications)]
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApproveApplicationResult>> Approve(
        Guid id, [FromBody] ApproveApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveApplicationCommand(
            id, request.CohortId, request.MentorId, request.TrackInterest, request.DecisionNote), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.ApproveApplications)]
    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(
        Guid id, [FromBody] DeclineApplicationRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeclineApplicationCommand(id, request.DecisionNote), cancellationToken);
        return NoContent();
    }
}
