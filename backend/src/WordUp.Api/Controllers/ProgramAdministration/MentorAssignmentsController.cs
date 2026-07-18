using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Api.Contracts.ProgramAdministration;
using WordUp.Application.ProgramAdministration.MentorAssignments;
using WordUp.Application.ProgramAdministration.People;

namespace WordUp.Api.Controllers.ProgramAdministration;

[ApiController]
[Route("api/mentor-assignments")]
[Authorize(Policy = Policies.ManageUsers)]
public sealed class MentorAssignmentsController : ControllerBase
{
    private readonly ISender _sender;

    public MentorAssignmentsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<MentorAssignmentOverview>> Get(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MentorAssignmentOverviewQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("youth/{youthId:guid}")]
    public async Task<IActionResult> AssignYouth(
        Guid youthId,
        [FromBody] AssignMentorRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new AssignMentorCommand(youthId, request.MentorId), cancellationToken);
        return NoContent();
    }

    [HttpPut("cohorts/{cohortId:guid}")]
    public async Task<IActionResult> SetCohortMentors(
        Guid cohortId,
        [FromBody] SetCohortMentorsRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SetCohortMentorsCommand(
                cohortId,
                request.LeadMentorId,
                request.SupportingMentorIds),
            cancellationToken);
        return NoContent();
    }
}
