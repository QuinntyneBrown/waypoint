using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ReportingAndData.DataTransfer;

public record PreviewRosterImportCommand(
    string CsvContent,
    IReadOnlyDictionary<string, string> ColumnMapping) : IRequest<RosterPreview>;

/// <summary>Status is one of: Ready, Duplicate, MissingConsent.</summary>
public record RosterPreviewRow(int Line, string Name, string Email, string Cohort, string Status);

public record RosterPreview(IReadOnlyList<RosterPreviewRow> Rows, int Ready, int Duplicates, int NeedsConsent);

public class PreviewRosterImportCommandValidator : AbstractValidator<PreviewRosterImportCommand>
{
    public PreviewRosterImportCommandValidator()
    {
        RuleFor(x => x.CsvContent).NotEmpty().WithMessage("CSV content is required.");
    }
}

public class PreviewRosterImportCommandHandler : IRequestHandler<PreviewRosterImportCommand, RosterPreview>
{
    private readonly IAppDbContext _db;

    public PreviewRosterImportCommandHandler(IAppDbContext db) => _db = db;

    public async Task<RosterPreview> Handle(PreviewRosterImportCommand request, CancellationToken cancellationToken)
    {
        var rows = RosterCsv.BuildRows(request.CsvContent, request.ColumnMapping);

        var existingEmails = await _db.Users
            .Select(u => u.Email.ToLower())
            .ToListAsync(cancellationToken);
        var existing = existingEmails.ToHashSet();

        var previewRows = new List<RosterPreviewRow>(rows.Count);
        int ready = 0, duplicates = 0, needsConsent = 0;

        foreach (var r in rows)
        {
            string status;
            if (!string.IsNullOrWhiteSpace(r.Email) && existing.Contains(r.Email.ToLowerInvariant()))
            {
                status = "Duplicate";
                duplicates++;
            }
            else if (r.IsMissingConsent)
            {
                status = "MissingConsent";
                needsConsent++;
            }
            else
            {
                status = "Ready";
                ready++;
            }

            previewRows.Add(new RosterPreviewRow(r.Line, r.Name, r.Email, r.Cohort, status));
        }

        return new RosterPreview(previewRows, ready, duplicates, needsConsent);
    }
}

/// <summary>A CSV data row resolved against the caller's column mapping.</summary>
internal sealed record MappedRosterRow(int Line, string Name, string Email, string Cohort, bool IsMissingConsent);

/// <summary>
/// Maps raw CSV rows onto system fields using the caller-supplied header=&gt;field mapping
/// (fields in {Name, Email, Cohort, Mentor, Track, Status, Consent, Skip}; unmapped/Skip ignored).
/// A row is MissingConsent when its mapped Status value equals "MissingConsent" (case-insensitive)
/// or a mapped Consent column equals "no" (case-insensitive).
/// </summary>
internal static class RosterCsv
{
    public static IReadOnlyList<MappedRosterRow> BuildRows(
        string content,
        IReadOnlyDictionary<string, string> mapping)
    {
        var parsed = Csv.Parse(content ?? string.Empty);
        var result = new List<MappedRosterRow>();
        if (parsed.Count == 0)
        {
            return result;
        }

        var headers = parsed[0];
        int nameIdx = -1, emailIdx = -1, cohortIdx = -1, statusIdx = -1, consentIdx = -1;

        for (var c = 0; c < headers.Count; c++)
        {
            if (mapping is null || !mapping.TryGetValue(headers[c], out var field))
            {
                continue;
            }

            switch (field?.Trim().ToLowerInvariant())
            {
                case "name": nameIdx = c; break;
                case "email": emailIdx = c; break;
                case "cohort": cohortIdx = c; break;
                case "status": statusIdx = c; break;
                case "consent": consentIdx = c; break;
                // mentor / track / skip / unmapped -> ignored
            }
        }

        for (var line = 1; line < parsed.Count; line++)
        {
            var cells = parsed[line];
            string Get(int idx) => idx >= 0 && idx < cells.Count ? cells[idx].Trim() : string.Empty;

            var statusVal = Get(statusIdx);
            var consentVal = Get(consentIdx);
            var missingConsent =
                string.Equals(statusVal, "MissingConsent", StringComparison.OrdinalIgnoreCase)
                || string.Equals(consentVal, "no", StringComparison.OrdinalIgnoreCase);

            result.Add(new MappedRosterRow(line + 1, Get(nameIdx), Get(emailIdx), Get(cohortIdx), missingConsent));
        }

        return result;
    }
}
