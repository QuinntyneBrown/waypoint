using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Messaging;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator) => _mediator = mediator;

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
        var result = await _mediator.Send(new BroadcastToGroupCommand(request.Body), cancellationToken);
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

public record SendMessageRequest(Guid ToUserId, string Body);

public record BroadcastRequest(string Body);
