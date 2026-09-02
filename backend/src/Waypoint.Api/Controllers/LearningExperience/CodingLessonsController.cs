using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Api.Contracts.LearningExperience;
using Waypoint.Application.LearningExperience.CodingLessons;
using Waypoint.Application.LearningExperience.Submissions;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Policy = Policies.YouthParticipation)]
[Route("api/me/coding-lessons")]
public sealed class CodingLessonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CodingLessonsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{lessonId:guid}")]
    public async Task<ActionResult<CodingLessonView>> Get(Guid lessonId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new CodingLessonQuery(lessonId), cancellationToken));

    [HttpPut("{lessonId:guid}/progress")]
    public async Task<ActionResult<CodingProgressResult>> Save(
        Guid lessonId,
        [FromBody] SaveCodingProgressRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(
            new SaveCodingProgressCommand(lessonId, request.Code, request.CheckedStepIds),
            cancellationToken));

    [HttpPost("{lessonId:guid}/submissions")]
    public async Task<ActionResult<SubmissionResult>> Submit(
        Guid lessonId,
        [FromBody] SubmitCodingProjectRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(
            new SubmitCodingProjectCommand(lessonId, request.Code, request.MentorNote, request.Attest),
            cancellationToken));
}
