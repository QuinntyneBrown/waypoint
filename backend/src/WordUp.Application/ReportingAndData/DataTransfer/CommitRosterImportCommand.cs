using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ReportingAndData.DataTransfer;

public record CommitRosterImportCommand(
    string CsvContent,
    IReadOnlyDictionary<string, string> ColumnMapping) : IRequest<RosterImportResult>;

public record RosterImportResult(int Imported, int SkippedDuplicates, int NeedsConsent);

public class CommitRosterImportCommandValidator : AbstractValidator<CommitRosterImportCommand>
{
    public CommitRosterImportCommandValidator()
    {
        RuleFor(x => x.CsvContent).NotEmpty().WithMessage("CSV content is required.");
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

        int imported = 0, skippedDuplicates = 0, needsConsent = 0;

        foreach (var r in rows)
        {
            var email = r.Email.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                // Nothing to key on — not importable.
                continue;
            }

            var lowered = email.ToLowerInvariant();
            var existing = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == lowered, cancellationToken);

            if (existing is not null)
            {
                // Idempotent by email: refresh the display name, never create a duplicate account.
                if (!string.IsNullOrWhiteSpace(r.Name))
                {
                    existing.DisplayName = r.Name;
                }

                skippedDuplicates++;
                continue;
            }

            if (r.IsMissingConsent)
            {
                // Counted but not activated (no account created without consent).
                needsConsent++;
                continue;
            }

            _db.Users.Add(new User
            {
                Email = email,
                DisplayName = string.IsNullOrWhiteSpace(r.Name) ? email : r.Name,
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CreatedAt = _clock.UtcNow,
            });
            imported++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("RosterImported", _currentUser.UserId, cancellationToken: cancellationToken);

        return new RosterImportResult(imported, skippedDuplicates, needsConsent);
    }
}
