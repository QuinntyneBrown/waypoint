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

    // Record a data-subject request (access / correction / removal / consent-withdrawal) for the caller (L2-NFR-003).
    [HttpPost("api/me/data-requests")]
    public async Task<ActionResult<DataSubjectRequestResult>> RequestDataSubjectAction(
        [FromBody] DataSubjectActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RequestDataSubjectActionCommand(request.ActionType, request.Details),
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
}
