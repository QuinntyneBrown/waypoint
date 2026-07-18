using WordUp.Api.Contracts.Mentoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Mentoring.AssignWork;

namespace WordUp.Api.Controllers.Mentoring;

[ApiController]
[Route("api/mentor")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class AssignWorkController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignWorkController(IMediator mediator) => _mediator = mediator;

    [HttpGet("assign-work/options")]
    public async Task<ActionResult<AssignWorkOptions>> Options(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new AssignWorkOptionsQuery(), cancellationToken));

    [HttpPost("assign-work")]
    public async Task<ActionResult<AssignWorkResult>> AssignWork(
        [FromBody] AssignWorkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignWorkCommand(
                request.ItemType,
                request.ItemId,
                request.YouthIds,
                request.CohortId,
                request.DueDate,
                request.ReminderCadence,
                request.Message),
            cancellationToken);
        return Ok(result);
    }
}
