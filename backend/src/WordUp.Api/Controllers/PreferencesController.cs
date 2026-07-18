using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.DisplayPreferences;
using WordUp.Application.NotificationPreferences;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize]
public class PreferencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreferencesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/me/notification-preferences")]
    public async Task<ActionResult<IReadOnlyList<NotificationPreferenceView>>> GetNotificationPreferences(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new NotificationPreferencesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/me/notification-preferences")]
    public async Task<IActionResult> SaveNotificationPreference(
        [FromBody] SaveNotificationPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SaveNotificationPreferenceCommand(request.EventKey, request.InApp, request.Email),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("api/me/display-preferences")]
    public async Task<ActionResult<DisplayPreferenceView>> GetDisplayPreferences(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DisplayPreferenceQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("api/me/display-preferences")]
    public async Task<ActionResult<DisplayPreferenceView>> SaveDisplayPreference(
        [FromBody] SaveDisplayPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SaveDisplayPreferenceCommand(
                request.TextScalePercent,
                request.HighContrast,
                request.ReduceMotion,
                request.UnderlineLinks,
                request.ReadingFont),
            cancellationToken);
        return Ok(result);
    }
}

public record SaveNotificationPreferenceRequest(string EventKey, bool InApp, bool Email);

public record SaveDisplayPreferenceRequest(
    int TextScalePercent,
    bool HighContrast,
    bool ReduceMotion,
    bool UnderlineLinks,
    string? ReadingFont);
