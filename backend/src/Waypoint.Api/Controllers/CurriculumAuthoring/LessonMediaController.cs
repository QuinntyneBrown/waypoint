using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.CurriculumAuthoring.Curriculum;

namespace Waypoint.Api.Controllers.CurriculumAuthoring;

[ApiController]
[Route("api/lesson-media")]
[Authorize]
public sealed class LessonMediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonMediaController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var media = await _mediator.Send(new LessonMediaFileQuery(id), cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        var result = new FileContentResult(media.Content, media.ContentType)
        {
            EnableRangeProcessing = true,
        };
        if (media.MediaType == "document")
        {
            result.FileDownloadName = media.FileName;
        }
        return result;
    }
}
