using WordUp.Api.Contracts.ProgramAdministration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.ProgramAdministration.Cohorts;

namespace WordUp.Api.Controllers.ProgramAdministration;

[ApiController]
[Route("api/cohorts")]
[Authorize(Policy = Policies.ManageUsers)]
public class CohortsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CohortsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<CohortResult>> Create([FromBody] CreateCohortRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCohortCommand(
                request.Name,
                request.TrackId,
                request.LeadMentorId,
                request.StartDate,
                request.EndDate,
                request.Location),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CohortSummary>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListCohortsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/youth")]
    public async Task<IActionResult> AddYouth(Guid id, [FromBody] AddYouthRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AddYouthToCohortCommand(id, request.YouthId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCohortRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new EditCohortCommand(id, request.Name, request.LeadMentorId, request.Location), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<ProgressBoardResult>> Progress(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProgressBoardQuery(id), cancellationToken);
        return Ok(result);
    }
}
