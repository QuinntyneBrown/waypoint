using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Reflections;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

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

public record SaveReflectionRequest(Guid LessonId, string? Text, string? Mood, bool ShareWithMentor, bool IsDraft);

public record UpdateReflectionRequest(string? Text, string? Mood, bool ShareWithMentor, bool IsDraft);
