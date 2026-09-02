using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.Engagement.Notifications;

namespace Waypoint.Api.Controllers.Engagement;

[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/notifications")]
    public async Task<ActionResult<IReadOnlyList<NotificationView>>> Feed(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new NotificationFeedQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/notifications/read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }
}
