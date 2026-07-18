using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.CompetencyAssessing;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Mentor + "," + Roles.ProgramAdministrator)]
public class CompetenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompetenciesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/mentor/youth/{youthId:guid}/competencies")]
    public async Task<IActionResult> SetLevel(
        Guid youthId,
        [FromBody] SetCompetencyLevelRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CompetencyLevel>(request.Level, ignoreCase: true, out var level))
        {
            ModelState.AddModelError(nameof(request.Level), "Unknown competency level.");
            return ValidationProblem(ModelState);
        }

        await _mediator.Send(
            new SetCompetencyLevelCommand(youthId, request.CompetencyId, level, request.Notes),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("api/mentor/youth/{youthId:guid}/competencies")]
    public async Task<ActionResult<CompetencySummary>> Get(Guid youthId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new YouthCompetenciesQuery(youthId), cancellationToken);
        return Ok(result);
    }
}

public record SetCompetencyLevelRequest(Guid CompetencyId, string Level, string? Notes);
