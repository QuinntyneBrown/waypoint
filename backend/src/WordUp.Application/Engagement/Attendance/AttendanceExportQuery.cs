using System.Globalization;
using System.Text;
using FluentValidation;
using MediatR;

namespace WordUp.Application.Engagement.Attendance;

public sealed record AttendanceExportQuery(
    string Format,
    string View,
    Guid? CohortId,
    DateTime? From,
    DateTime? To) : IRequest<AttendanceExportFile>;

public sealed record AttendanceExportFile(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed class AttendanceExportQueryHandler
    : IRequestHandler<AttendanceExportQuery, AttendanceExportFile>
{
    private readonly ISender _sender;

    public AttendanceExportQueryHandler(ISender sender) => _sender = sender;

    public async Task<AttendanceExportFile> Handle(
        AttendanceExportQuery request,
        CancellationToken cancellationToken)
    {
        var format = request.Format.Trim().ToLowerInvariant();
        var view = request.View.Trim().ToLowerInvariant();
        if (format is not ("csv" or "pdf"))
        {
            throw new ValidationException("Export format must be csv or pdf.");
        }
        if (view is not ("session" or "youth"))
        {
            throw new ValidationException("Attendance view must be session or youth.");
        }

        var lines = view == "session"
            ? await SessionLinesAsync(request, cancellationToken)
            : await YouthLinesAsync(request, cancellationToken);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        if (format == "csv")
        {
            return new AttendanceExportFile(
                $"attendance-{view}-{stamp}.csv",
                "text/csv; charset=utf-8",
                Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines) + Environment.NewLine));
        }

        return new AttendanceExportFile(
            $"attendance-{view}-{stamp}.pdf",
            "application/pdf",
            SimplePdf.Create($"Word Up attendance by {view}", lines));
    }

    private async Task<IReadOnlyList<string>> SessionLinesAsync(
        AttendanceExportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _sender.Send(
            new AttendanceHistoryBySessionQuery(request.CohortId, request.From, request.To),
            cancellationToken);
        var lines = new List<string>
        {
            Csv("Date", "Session", "Cohort", "Location", "Present", "Marked", "Roster", "Rate", "Status"),
        };
        lines.AddRange(rows.Select(row => Csv(
            row.StartsAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            row.Title,
            row.CohortName,
            row.Location ?? string.Empty,
            row.Present.ToString(CultureInfo.InvariantCulture),
            row.Marked.ToString(CultureInfo.InvariantCulture),
            row.RosterTotal.ToString(CultureInfo.InvariantCulture),
            row.Rate.ToString("P0", CultureInfo.InvariantCulture),
            row.Status)));
        return lines;
    }

    private async Task<IReadOnlyList<string>> YouthLinesAsync(
        AttendanceExportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _sender.Send(
            new AttendanceHistoryYouthListQuery(request.CohortId, request.From, request.To),
            cancellationToken);
        var lines = new List<string>
        {
            Csv("Youth", "Attended", "Late", "Excused", "Absent", "Total", "Rate", "Trend", "Below threshold"),
        };
        lines.AddRange(rows.Select(row => Csv(
            row.DisplayName,
            row.Attended.ToString(CultureInfo.InvariantCulture),
            row.Late.ToString(CultureInfo.InvariantCulture),
            row.Excused.ToString(CultureInfo.InvariantCulture),
            row.Absent.ToString(CultureInfo.InvariantCulture),
            row.Total.ToString(CultureInfo.InvariantCulture),
            row.Rate.ToString("P0", CultureInfo.InvariantCulture),
            row.Trend,
            row.BelowThreshold ? "Yes" : "No")));
        return lines;
    }

    private static string Csv(params string[] fields) => string.Join(",", fields.Select(field =>
        field.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{field.Replace("\"", "\"\"")}\""
            : field));
}

internal static class SimplePdf
{
    public static byte[] Create(string title, IReadOnlyList<string> lines)
    {
        var printable = new[] { title, $"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", string.Empty }
            .Concat(lines)
            .Select(line => line.Length > 100 ? line[..97] + "..." : line)
            .ToList();
        var pages = printable.Chunk(48).ToList();
        if (pages.Count == 0) pages.Add([]);

        var objects = new Dictionary<int, string>();
        objects[1] = "<< /Type /Catalog /Pages 2 0 R >>";
        objects[3] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>";
        var pageIds = new List<int>();
        for (var index = 0; index < pages.Count; index++)
        {
            var pageId = 4 + index * 2;
            var streamId = pageId + 1;
            pageIds.Add(pageId);
            var stream = BuildStream(pages[index]);
            objects[pageId] =
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
                $"/Resources << /Font << /F1 3 0 R >> >> /Contents {streamId} 0 R >>";
            objects[streamId] =
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream";
        }
        objects[2] =
            $"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] " +
            $"/Count {pageIds.Count} >>";

        using var output = new MemoryStream();
        Write(output, "%PDF-1.4\n");
        var maxId = objects.Keys.Max();
        var offsets = new long[maxId + 1];
        for (var id = 1; id <= maxId; id++)
        {
            offsets[id] = output.Position;
            Write(output, $"{id} 0 obj\n{objects[id]}\nendobj\n");
        }
        var xref = output.Position;
        Write(output, $"xref\n0 {maxId + 1}\n0000000000 65535 f \n");
        for (var id = 1; id <= maxId; id++)
        {
            Write(output, $"{offsets[id]:D10} 00000 n \n");
        }
        Write(output,
            $"trailer\n<< /Size {maxId + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return output.ToArray();
    }

    private static string BuildStream(IEnumerable<string> lines)
    {
        var builder = new StringBuilder("BT\n/F1 10 Tf\n40 760 Td\n14 TL\n");
        foreach (var line in lines)
        {
            var ascii = new string(line.Select(character => character <= 126 ? character : '?').ToArray());
            var escaped = ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            builder.Append('(').Append(escaped).Append(") Tj\nT*\n");
        }
        builder.Append("ET");
        return builder.ToString();
    }

    private static void Write(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
