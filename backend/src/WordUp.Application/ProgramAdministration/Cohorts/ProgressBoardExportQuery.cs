using System.Globalization;
using System.Text;
using FluentValidation;
using MediatR;
using WordUp.Application.Engagement.Attendance;
using WordUp.Application.ReportingAndData.DataTransfer;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record ProgressBoardExportQuery(string Format, Guid CohortId, Guid? TrackId)
    : IRequest<ProgressBoardExportFile>;

public record ProgressBoardExportFile(string FileName, string ContentType, byte[] Content);

public sealed class ProgressBoardExportQueryHandler
    : IRequestHandler<ProgressBoardExportQuery, ProgressBoardExportFile>
{
    private readonly ISender _sender;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ProgressBoardExportQueryHandler(
        ISender sender,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ProgressBoardExportFile> Handle(
        ProgressBoardExportQuery request,
        CancellationToken cancellationToken)
    {
        var format = request.Format.Trim().ToLowerInvariant();
        if (format is not ("csv" or "pdf"))
        {
            throw new ValidationException("Export format must be csv or pdf.");
        }

        var board = await _sender.Send(
            new ProgressBoardQuery(request.CohortId, request.TrackId),
            cancellationToken);
        var header = new[] { "Youth" }
            .Concat(board.Modules.Select(module => module.Name))
            .Concat(["Quiz average", "Overall"]);
        var csvRows = new List<string> { Csv.ToRow(header) };
        csvRows.AddRange(board.Rows.Select(row => Csv.ToRow(
            new[] { row.YouthName }
                .Concat(row.Cells.Select(cell => cell.State))
                .Concat([
                    row.QuizAverage.ToString("0.#", CultureInfo.InvariantCulture) + "%",
                    row.OverallPercent.ToString("P0", CultureInfo.InvariantCulture),
                ]))));
        csvRows.Add(Csv.ToRow(
            new[] { "Cohort average" }
                .Concat(board.Modules.Select(module =>
                    module.AveragePercent.ToString("P0", CultureInfo.InvariantCulture)))
                .Concat([string.Empty, board.CohortAveragePercent.ToString("P0", CultureInfo.InvariantCulture)])));

        var extension = format;
        var fileName = $"progress-{board.CohortId:N}-{board.TrackId:N}.{extension}";
        ProgressBoardExportFile file;
        if (format == "csv")
        {
            file = new ProgressBoardExportFile(
                fileName,
                "text/csv; charset=utf-8",
                Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, csvRows) + Environment.NewLine));
        }
        else
        {
            var lines = new List<string>
            {
                $"Cohort: {board.CohortName}",
                $"Track: {board.TrackName}",
                string.Empty,
            };
            lines.AddRange(csvRows);
            file = new ProgressBoardExportFile(
                fileName,
                "application/pdf",
                SimplePdf.Create("Word Up progress board", lines));
        }

        await _audit.LogAsync(
            "DataExported",
            _currentUser.UserId,
            "CohortProgress",
            board.CohortId,
            details: board.TrackId.ToString(),
            cancellationToken: cancellationToken);
        return file;
    }
}
