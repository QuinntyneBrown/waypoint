using Waypoint.Api.Contracts.PlatformServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Application.PlatformServices.Files;
using Waypoint.Application.PlatformServices.Support;

namespace Waypoint.Api.Controllers.PlatformServices;

[ApiController]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupportController(IMediator mediator) => _mediator = mediator;

    [HttpPost("api/support")]
    public async Task<ActionResult<SupportRequestResult>> Submit(
        [FromBody] SubmitSupportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitSupportRequestCommand(
                request.Category,
                request.Description,
                request.ReplyToEmail,
                request.ScreenshotFileId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/support/screenshots")]
    [RequestSizeLimit(SafeImageUploadPolicy.MaxBytes + 65536)]
    public async Task<ActionResult<SupportScreenshotResult>> UploadScreenshot(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return Ok(await _mediator.Send(
            new StoreSupportScreenshotCommand(
                file.FileName,
                file.ContentType.ToLowerInvariant(),
                stream.ToArray()),
            cancellationToken));
    }

    [HttpGet("api/support/screenshots/{id:guid}")]
    public async Task<IActionResult> Screenshot(Guid id, CancellationToken cancellationToken)
    {
        var screenshot = await _mediator.Send(new GetSupportScreenshotQuery(id), cancellationToken);
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(screenshot.Content, screenshot.ContentType, screenshot.FileName);
    }

    [HttpDelete("api/support/screenshots/{id:guid}")]
    public async Task<IActionResult> RemoveScreenshot(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveSupportScreenshotCommand(id), cancellationToken);
        return NoContent();
    }
}
