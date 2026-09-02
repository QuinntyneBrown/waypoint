using FluentValidation;
using MediatR;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ReportingAndData.DataTransfer;

public record CommitRosterImportCommand(
    string CsvContent,
    IReadOnlyDictionary<string, string> ColumnMapping) : IRequest<RosterImportResult>;

public record RosterImportResult(int Imported, int SkippedDuplicates, int NeedsConsent, int Invalid);

public class CommitRosterImportCommandValidator : AbstractValidator<CommitRosterImportCommand>
{
    public CommitRosterImportCommandValidator()
    {
        RosterImportValidation.Apply(this);
    }
}

public class CommitRosterImportCommandHandler : IRequestHandler<CommitRosterImportCommand, RosterImportResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CommitRosterImportCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<RosterImportResult> Handle(CommitRosterImportCommand request, CancellationToken cancellationToken)
    {
        var rows = RosterCsv.BuildRows(request.CsvContent, request.ColumnMapping);

        var lookup = await RosterImportLookup.LoadAsync(_db, cancellationToken);
        int imported = 0, skippedDuplicates = 0, needsConsent = 0, invalid = 0;

        foreach (var row in rows)
        {
            var classified = lookup.Classify(row);
            switch (classified.Status)
            {
                case "Duplicate":
                    skippedDuplicates++;
                    continue;
                case "MissingConsent":
                    needsConsent++;
                    continue;
                case "Invalid":
                    invalid++;
                    continue;
            }

            var youth = new User
            {
                Email = row.Email.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(row.Name) ? row.Email.Trim() : row.Name,
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = classified.CohortId,
                MentorId = classified.MentorId,
                CreatedAt = _clock.UtcNow,
            };
            _db.Users.Add(youth);
            foreach (var trackId in classified.TrackIds)
            {
                _db.YouthTrackEnrollments.Add(new YouthTrackEnrollment
                {
                    YouthUserId = youth.Id,
                    TrackId = trackId,
                    EnrolledAt = _clock.UtcNow,
                });
            }
            imported++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "RosterImported",
            _currentUser.UserId,
            "Roster",
            details: $"imported={imported};duplicates={skippedDuplicates};needsConsent={needsConsent};invalid={invalid}",
            cancellationToken: cancellationToken);

        return new RosterImportResult(imported, skippedDuplicates, needsConsent, invalid);
    }
}
