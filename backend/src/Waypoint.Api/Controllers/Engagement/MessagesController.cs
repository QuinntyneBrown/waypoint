using Waypoint.Api.Contracts.Engagement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.Engagement.Messaging;

namespace Waypoint.Api.Controllers.Engagement;

[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/mentor/messages")]
    [Authorize(Roles = Roles.Mentor)]
    public async Task<ActionResult<MentorMessageInboxView>> MentorInbox(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new MentorMessageInboxQuery(), cancellationToken));

    [HttpGet("api/me/messages")]
    [Authorize(Roles = Roles.Youth)]
    public async Task<ActionResult<YouthMessageThreadView>> Mine(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new YouthMessageThreadQuery(), cancellationToken));

    [HttpPost("api/messages")]
    public async Task<ActionResult<MessageResult>> Send(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendMessageCommand(request.ToUserId, request.Body), cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/messages/broadcast")]
    [Authorize(Roles = Roles.Mentor)]
    public async Task<ActionResult<BroadcastResult>> Broadcast(
        [FromBody] BroadcastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new BroadcastToGroupCommand(request.CohortId, request.Body),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/messages/{otherUserId:guid}")]
    public async Task<ActionResult<IReadOnlyList<MessageView>>> Thread(
        Guid otherUserId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MessageThreadQuery(otherUserId), cancellationToken);
        return Ok(result);
    }
}
