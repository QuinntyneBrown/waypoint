using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.CheckIns;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class CheckInsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckInsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/mentor/check-ins")]
    public async Task<ActionResult<CheckInResult>> Log(
        [FromBody] LogCheckInRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LogCheckInCommand(
                request.YouthId,
                request.Date,
                request.Type,
                request.Engagement,
                request.GoalsDiscussed,
                request.PrayerRequests,
                request.ActionItems,
                request.NextFollowUp,
                request.ShareWithMentor,
                request.GuardianInformed),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/mentor/check-ins")]
    public async Task<ActionResult<IReadOnlyList<CheckInSummary>>> History(
        [FromQuery] Guid? youthId = null,
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CheckInHistoryQuery(youthId, type), cancellationToken);
        return Ok(result);
    }
}

public record LogCheckInRequest(
    Guid YouthId,
    DateTime Date,
    string Type,
    string? Engagement,
    string? GoalsDiscussed,
    string? PrayerRequests,
    string? ActionItems,
    DateTime? NextFollowUp,
    bool ShareWithMentor,
    bool GuardianInformed);
