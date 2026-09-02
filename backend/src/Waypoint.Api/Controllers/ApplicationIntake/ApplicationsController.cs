using Waypoint.Api.Contracts.ApplicationIntake;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.ApplicationIntake;

namespace Waypoint.Api.Controllers.ApplicationIntake;

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
            request.WhoIsFilling,
            request.Neighbourhood,
            request.CohortInterest,
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
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationSummary>>> List(
        [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListApplicationsQuery(status), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.ApproveApplications)]
    [HttpGet("options")]
    public async Task<ActionResult<ApplicationReviewOptions>> Options(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ApplicationReviewOptionsQuery(), cancellationToken));

    [Authorize(Policy = Policies.ApproveApplications)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationDetailView>> Detail(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ApplicationDetailQuery(id), cancellationToken));

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
