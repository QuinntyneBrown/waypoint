using System.Net.Mail;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ReportingAndData.DataTransfer;

public record PreviewRosterImportCommand(
    string CsvContent,
    IReadOnlyDictionary<string, string> ColumnMapping) : IRequest<RosterPreview>;

/// <summary>Status is one of: Ready, Duplicate, MissingConsent, Invalid.</summary>
public record RosterPreviewRow(
    int Line,
    string Name,
    string Email,
    string Cohort,
    string Mentor,
    string Track,
    string Status,
    string? Message);

public record RosterPreview(
    IReadOnlyList<RosterPreviewRow> Rows,
    int Ready,
    int Duplicates,
    int NeedsConsent,
    int Invalid);

public class PreviewRosterImportCommandValidator : AbstractValidator<PreviewRosterImportCommand>
{
    public PreviewRosterImportCommandValidator()
    {
        RosterImportValidation.Apply(this);
    }
}

public class PreviewRosterImportCommandHandler : IRequestHandler<PreviewRosterImportCommand, RosterPreview>
{
    private readonly IAppDbContext _db;

    public PreviewRosterImportCommandHandler(IAppDbContext db) => _db = db;

    public async Task<RosterPreview> Handle(PreviewRosterImportCommand request, CancellationToken cancellationToken)
    {
        var rows = RosterCsv.BuildRows(request.CsvContent, request.ColumnMapping);
        var lookup = await RosterImportLookup.LoadAsync(_db, cancellationToken);

        var previewRows = new List<RosterPreviewRow>(rows.Count);
        int ready = 0, duplicates = 0, needsConsent = 0, invalid = 0;

        foreach (var row in rows)
        {
            var classified = lookup.Classify(row);
            switch (classified.Status)
            {
                case "Ready": ready++; break;
                case "Duplicate": duplicates++; break;
                case "MissingConsent": needsConsent++; break;
                default: invalid++; break;
            }
            previewRows.Add(new RosterPreviewRow(
                row.Line,
                row.Name,
                row.Email,
                row.Cohort,
                row.Mentor,
                row.Track,
                classified.Status,
                classified.Message));
        }

        return new RosterPreview(previewRows, ready, duplicates, needsConsent, invalid);
    }
}

/// <summary>A CSV data row resolved against the caller's column mapping.</summary>
internal sealed record MappedRosterRow(
    int Line,
    string Name,
    string Email,
    string Cohort,
    string Mentor,
    string Track,
    bool IsMissingConsent);

internal static class RosterImportValidation
{
    public const int MaximumBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedFields = new(
        ["Name", "Email", "Cohort", "Mentor", "Track", "Status", "Consent", "Skip"],
        StringComparer.OrdinalIgnoreCase);

    public static void Apply<T>(AbstractValidator<T> validator)
        where T : class
    {
        validator.RuleFor(command => Content(command))
            .NotEmpty().WithMessage("CSV content is required.")
            .Must(content => Encoding.UTF8.GetByteCount(content ?? string.Empty) <= MaximumBytes)
            .WithMessage("CSV content must be 2 MB or smaller.");
        validator.RuleFor(command => Mapping(command))
            .NotNull().WithMessage("Column mapping is required.")
            .Must(mapping => mapping is not null
                && mapping.Values.Count(value => value.Equals("Email", StringComparison.OrdinalIgnoreCase)) == 1)
            .WithMessage("Map exactly one column to Email.")
            .Must(mapping => mapping is not null && mapping.Values.All(value => AllowedFields.Contains(value)))
            .WithMessage("Column mappings must use a supported Waypoint field.")
            .Must(mapping => mapping is not null && mapping.Values
                .Where(value => !value.Equals("Skip", StringComparison.OrdinalIgnoreCase))
                .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .All(group => group.Count() == 1))
            .WithMessage("Map each Waypoint field at most once.");
    }

    private static string Content<T>(T command) => command switch
    {
        PreviewRosterImportCommand preview => preview.CsvContent,
        CommitRosterImportCommand commit => commit.CsvContent,
        _ => string.Empty,
    };

    private static IReadOnlyDictionary<string, string> Mapping<T>(T command) => command switch
    {
        PreviewRosterImportCommand preview => preview.ColumnMapping,
        CommitRosterImportCommand commit => commit.ColumnMapping,
        _ => new Dictionary<string, string>(),
    };
}

internal sealed record ClassifiedRosterRow(
    MappedRosterRow Row,
    string Status,
    string? Message,
    Guid? CohortId,
    Guid? MentorId,
    IReadOnlyList<Guid> TrackIds);

internal sealed class RosterImportLookup
{
    private readonly HashSet<string> _seenEmails;
    private readonly IReadOnlyDictionary<string, Cohort> _cohorts;
    private readonly IReadOnlyDictionary<string, User> _mentors;
    private readonly IReadOnlyDictionary<string, Track> _tracks;

    private RosterImportLookup(
        HashSet<string> seenEmails,
        IReadOnlyDictionary<string, Cohort> cohorts,
        IReadOnlyDictionary<string, User> mentors,
        IReadOnlyDictionary<string, Track> tracks)
    {
        _seenEmails = seenEmails;
        _cohorts = cohorts;
        _mentors = mentors;
        _tracks = tracks;
    }

    public static async Task<RosterImportLookup> LoadAsync(
        IAppDbContext db,
        CancellationToken cancellationToken)
    {
        var emails = await db.Users.AsNoTracking()
            .Select(item => item.Email.ToLower())
            .ToListAsync(cancellationToken);
        var cohorts = await db.Cohorts.AsNoTracking().ToListAsync(cancellationToken);
        var mentors = await db.Users.AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active)
            .ToListAsync(cancellationToken);
        var tracks = await db.Tracks.AsNoTracking()
            .Where(item => item.Status == PublishStatus.Published && item.Enabled)
            .ToListAsync(cancellationToken);
        return new RosterImportLookup(
            emails.ToHashSet(StringComparer.OrdinalIgnoreCase),
            UniqueByName(cohorts, item => item.Name),
            UniqueByName(mentors, item => item.DisplayName),
            UniqueByName(tracks, item => item.Name));
    }

    public ClassifiedRosterRow Classify(MappedRosterRow row)
    {
        var email = row.Email.Trim();
        if (!MailAddress.TryCreate(email, out var parsed)
            || !parsed.Address.Equals(email, StringComparison.OrdinalIgnoreCase))
            return Invalid(row, "Enter a valid email address.");
        if (_seenEmails.Contains(email))
            return new ClassifiedRosterRow(row, "Duplicate", "An account with this email already exists.", null, null, []);

        Cohort? cohort = null;
        if (!string.IsNullOrWhiteSpace(row.Cohort) && !_cohorts.TryGetValue(row.Cohort, out cohort))
            return Invalid(row, "Cohort was not found or is ambiguous.");
        User? mentor = null;
        if (!string.IsNullOrWhiteSpace(row.Mentor) && !_mentors.TryGetValue(row.Mentor, out mentor))
            return Invalid(row, "Mentor was not found, active, or uniquely named.");
        Track? track = null;
        if (!string.IsNullOrWhiteSpace(row.Track) && !_tracks.TryGetValue(row.Track, out track))
            return Invalid(row, "Track was not found, published, enabled, or uniquely named.");

        _seenEmails.Add(email);
        var trackIds = new[] { cohort?.TrackId, track?.Id }
            .Where(item => item is not null)
            .Select(item => item!.Value)
            .Distinct()
            .ToList();
        return row.IsMissingConsent
            ? new ClassifiedRosterRow(row, "MissingConsent", "Guardian consent requires follow-up.", cohort?.Id, mentor?.Id, trackIds)
            : new ClassifiedRosterRow(row, "Ready", null, cohort?.Id, mentor?.Id, trackIds);
    }

    private static ClassifiedRosterRow Invalid(MappedRosterRow row, string message) =>
        new(row, "Invalid", message, null, null, []);

    private static IReadOnlyDictionary<string, T> UniqueByName<T>(
        IEnumerable<T> values,
        Func<T, string> name) => values
        .GroupBy(name, StringComparer.OrdinalIgnoreCase)
        .Where(group => group.Count() == 1)
        .ToDictionary(group => group.Key, group => group.Single(), StringComparer.OrdinalIgnoreCase);
}

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
        int nameIdx = -1, emailIdx = -1, cohortIdx = -1, mentorIdx = -1, trackIdx = -1,
            statusIdx = -1, consentIdx = -1;

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
                case "mentor": mentorIdx = c; break;
                case "track": trackIdx = c; break;
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

            result.Add(new MappedRosterRow(
                line + 1,
                Get(nameIdx),
                Get(emailIdx),
                Get(cohortIdx),
                Get(mentorIdx),
                Get(trackIdx),
                missingConsent));
        }

        return result;
    }
}
