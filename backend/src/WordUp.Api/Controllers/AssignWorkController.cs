using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.AssignWork;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/mentor")]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class AssignWorkController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignWorkController(IMediator mediator) => _mediator = mediator;

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

public record AssignWorkRequest(
    string ItemType,
    Guid ItemId,
    IReadOnlyList<Guid>? YouthIds,
    Guid? CohortId,
    DateTime? DueDate,
    string? ReminderCadence,
    string? Message);
