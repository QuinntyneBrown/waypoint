using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Assessments;

namespace WordUp.Api.Controllers;

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
        var rubric = (request.Rubric ?? new List<RubricInput>())
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
}

public record CreateAssignmentRequest(
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    IReadOnlyList<RubricInput> Rubric);
