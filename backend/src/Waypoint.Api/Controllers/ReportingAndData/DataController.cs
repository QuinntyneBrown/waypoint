using Waypoint.Api.Contracts.ReportingAndData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Api.Authorization;
using Waypoint.Application.ReportingAndData.DataTransfer;

namespace Waypoint.Api.Controllers.ReportingAndData;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.ViewAuditLog)]
    [HttpGet("api/admin/audit/export.csv")]
    public async Task<ActionResult<CsvFile>> ExportAuditLog(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExportAuditLogQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.ViewAuditLog)]
    [HttpGet("api/admin/audit/export")]
    public async Task<IActionResult> ExportAuditLogFile(
        [FromQuery] string format,
        [FromQuery] string? action = null,
        [FromQuery] Guid? actorUserId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new ExportAuditLogFileQuery(format, action, actorUserId, from, to),
            cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [Authorize(Policy = Policies.ImportExportData)]
    [HttpPost("api/admin/import/preview")]
    public async Task<ActionResult<RosterPreview>> PreviewImport(
        [FromBody] RosterImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PreviewRosterImportCommand(
                request.CsvContent,
                request.ColumnMapping ?? new Dictionary<string, string>()),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.ImportExportData)]
    [HttpPost("api/admin/import")]
    public async Task<ActionResult<RosterImportResult>> CommitImport(
        [FromBody] RosterImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CommitRosterImportCommand(
                request.CsvContent,
                request.ColumnMapping ?? new Dictionary<string, string>()),
            cancellationToken);
        return Ok(result);
    }
}
