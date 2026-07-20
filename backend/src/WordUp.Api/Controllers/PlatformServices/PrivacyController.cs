using WordUp.Api.Contracts.PlatformServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.PlatformServices.Privacy;

namespace WordUp.Api.Controllers.PlatformServices;

[ApiController]
[Authorize]
public class PrivacyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrivacyController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/me/data-requests")]
    public async Task<ActionResult<MyDataSubjectRequestPage>> History(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(
            new MyDataSubjectRequestsQuery(page, pageSize),
            cancellationToken));
    }

    // Record a data-subject request (access / correction / removal / consent-withdrawal) for the caller (L2-NFR-003).
    [HttpPost("api/me/data-requests")]
    public async Task<ActionResult<DataSubjectRequestResult>> RequestDataSubjectAction(
        [FromBody] DataSubjectActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RequestDataSubjectActionCommand(
                request.ActionType,
                request.Details,
                request.Correction is null
                    ? null
                    : new DataSubjectCorrection(
                        request.Correction.DisplayName,
                        request.Correction.Email,
                        request.Correction.Pronouns,
                        request.Correction.ClearPronouns)),
            cancellationToken);
        return Ok(result);
    }

    // Data-subject "access" export: the caller's own stored personal data, scoped strictly to their identity.
    [HttpGet("api/me/data")]
    public async Task<ActionResult<MyDataResult>> MyData(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MyDataQuery(), cancellationToken);
        return Ok(result);
    }

    // Complete subject-access export. A completed administrator-reviewed access request is enforced
    // inside the query handler; this endpoint is never a shortcut around that workflow.
    [HttpGet("api/me/data/export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PersonalDataExportQuery(), cancellationToken);
        Response.Headers.CacheControl = "no-store, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(result.Content, "application/json; charset=utf-8", result.FileName);
    }
}
