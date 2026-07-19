using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.LearningExperience.Submissions;

namespace WordUp.Api.Controllers.LearningExperience;

[ApiController]
[Route("api/submission-files")]
[Authorize]
public sealed class SubmissionFilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionFilesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new SubmissionFileQuery(fileId), cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return new FileContentResult(file.Content, file.ContentType)
        {
            FileDownloadName = file.FileName,
        };
    }
}
