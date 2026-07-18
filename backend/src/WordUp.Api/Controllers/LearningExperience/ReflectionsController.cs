using WordUp.Api.Contracts.LearningExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.LearningExperience.Reflections;

namespace WordUp.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Roles = Roles.Youth)]
[Route("api/me/reflections")]
public class ReflectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReflectionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ReflectionResult>> Save([FromBody] SaveReflectionRequest request, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(
            new SaveReflectionCommand(request.LessonId, request.Text, request.Mood, request.ShareWithMentor, request.IsDraft),
            cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReflectionRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateReflectionCommand(id, request.Text, request.Mood, request.ShareWithMentor, request.IsDraft),
            cancellationToken);
        return NoContent();
    }
}
