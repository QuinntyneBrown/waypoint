using WordUp.Api.Contracts.ProgramAdministration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.ProgramAdministration.People;

namespace WordUp.Api.Controllers.ProgramAdministration;

[ApiController]
[Route("api/people")]
[Authorize(Policy = Policies.ManageUsers)]
public class PeopleController : ControllerBase
{
    private readonly IMediator _mediator;

    public PeopleController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedPeopleResult>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? role = null,
        [FromQuery] Guid? cohortId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListPeopleQuery(page, pageSize, role, cohortId, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonDetail>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPersonQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivatePersonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReactivatePersonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/remove")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemovePersonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ChangeRoleCommand(id, request.Role), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/send-password-reset")]
    public async Task<IActionResult> SendPasswordReset(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SendPasswordResetForPersonCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/assign-mentor")]
    public async Task<IActionResult> AssignMentor(Guid id, [FromBody] AssignMentorRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AssignMentorCommand(id, request.MentorId), cancellationToken);
        return NoContent();
    }
}
