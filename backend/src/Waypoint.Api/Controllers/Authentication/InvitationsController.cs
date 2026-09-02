using Waypoint.Api.Contracts.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Authentication.Invitations;

namespace Waypoint.Api.Controllers.Authentication;

[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpGet("options")]
    public async Task<ActionResult<InvitationOptions>> Options(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new InvitationOptionsQuery(), cancellationToken));
    }

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpGet]
    public async Task<ActionResult<InvitationPage>> List(
        [FromQuery] string? status,
        [FromQuery] string? role,
        [FromQuery] Guid? cohortId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new InvitationListQuery(status, role, cohortId, search, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("context")]
    public async Task<ActionResult<InvitationContext>> Context(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new InvitationContextQuery(token), cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("replacement-request")]
    public async Task<IActionResult> RequestReplacement(
        [FromBody] ReplacementInvitationRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RequestReplacementInvitationCommand(request.Token), cancellationToken);
        return Accepted();
    }

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpPost]
    public async Task<ActionResult<InviteResult>> Invite([FromBody] InviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new InviteCommand(request.Email, request.Role, request.CohortId, request.MentorId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkInviteResult>> Bulk([FromBody] BulkInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new BulkInviteCommand(request.Emails, request.Role, request.CohortId, request.MentorId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("accept")]
    public async Task<ActionResult<AuthResult>> Accept([FromBody] AcceptInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AcceptInviteCommand(request.Token, request.DisplayName, request.Password, request.AgreeToGuidelines),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpPost("{id:guid}/resend")]
    public async Task<IActionResult> Resend(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResendInvitationCommand(id), cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.InviteUsers)]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeInvitationCommand(id), cancellationToken);
        return NoContent();
    }
}
