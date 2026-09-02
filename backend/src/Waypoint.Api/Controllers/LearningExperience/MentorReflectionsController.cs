using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.LearningExperience.Reflections;

namespace Waypoint.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Roles = $"{Roles.Mentor},{Roles.ProgramAdministrator}")]
[Route("api/mentor/youth/{youthId:guid}/reflections")]
public sealed class MentorReflectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MentorReflectionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SharedReflectionView>>> List(
        Guid youthId,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new SharedReflectionsQuery(youthId), cancellationToken));
}
