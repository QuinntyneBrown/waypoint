using WordUp.Api.Contracts.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Authentication.Invitations;

namespace WordUp.Api.Controllers.Authentication;

[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator) => _mediator = mediator;

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
        var result = await _mediator.Send(new BulkInviteCommand(request.Emails, request.Role), cancellationToken);
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
