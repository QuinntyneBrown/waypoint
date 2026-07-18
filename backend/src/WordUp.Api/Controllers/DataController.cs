using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.DataTransfer;

namespace WordUp.Api.Controllers;

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

public record RosterImportRequest(string CsvContent, Dictionary<string, string>? ColumnMapping);
