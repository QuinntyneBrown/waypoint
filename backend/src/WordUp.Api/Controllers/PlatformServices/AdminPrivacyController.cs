using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Api.Contracts.PlatformServices;
using WordUp.Application.PlatformServices.Privacy;

namespace WordUp.Api.Controllers.PlatformServices;

[ApiController]
[Authorize(Policy = Policies.ManageUsers)]
[Route("api/admin")]
public sealed class AdminPrivacyController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPrivacyController(IMediator mediator) => _mediator = mediator;

    [HttpGet("data-requests")]
    public async Task<ActionResult<AdminDataSubjectRequestPage>> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new AdminDataSubjectRequestsQuery(status, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("data-requests/{id:guid}/fulfill")]
    public async Task<ActionResult<DataSubjectRequestResult>> Fulfill(
        Guid id,
        [FromBody] FulfillDataSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new FulfillDataSubjectRequestCommand(
                id,
                request.DisplayName,
                request.Email,
                request.Pronouns,
                request.ResolutionNote),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("privacy/retention")]
    public async Task<ActionResult<RetentionRunResult>> RunRetention(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RunRetentionCommand(), cancellationToken);
        return Ok(result);
    }
}
