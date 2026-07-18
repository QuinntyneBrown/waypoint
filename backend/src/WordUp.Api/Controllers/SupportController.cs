using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.Support;

namespace WordUp.Api.Controllers;

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
                request.ScreenshotUrl),
            cancellationToken);
        return Ok(result);
    }
}

public record SubmitSupportRequest(string Category, string Description, string ReplyToEmail, string? ScreenshotUrl);
