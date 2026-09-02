using Waypoint.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.CurriculumAuthoring.Assessments;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignmentsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost]
    public async Task<ActionResult<AssignmentResult>> Create([FromBody] CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var rubric = (request.Rubric ?? new List<RubricRequest>())
            .Select(r => new RubricInput(r.Description, r.Points))
            .ToList();

        var result = await _mediator.Send(
            new CreateAssignmentCommand(
                request.ModuleId, request.LessonId, request.Title, request.TaskDescription,
                request.SubmissionTypes, request.DueDate, request.GracePeriodDays,
                request.AllowLate, request.RequireMentorApproval, rubric),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentDetail>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetAssignmentQuery(id), cancellationToken));
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var rubric = (request.Rubric ?? [])
            .Select(item => new RubricInput(item.Description, item.Points))
            .ToList();
        await _mediator.Send(
            new UpdateAssignmentCommand(
                id,
                request.ModuleId,
                request.LessonId,
                request.Title,
                request.TaskDescription,
                request.SubmissionTypes,
                request.DueDate,
                request.GracePeriodDays,
                request.AllowLate,
                request.RequireMentorApproval,
                rubric),
            cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id,
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new PublishAssignmentCommand(id, request.Status),
            cancellationToken);
        return NoContent();
    }
}
