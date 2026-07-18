using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.Settings;

public sealed record ListTermsQuery() : IRequest<IReadOnlyList<TermView>>;
public sealed record TermView(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status);

public sealed class ListTermsQueryHandler : IRequestHandler<ListTermsQuery, IReadOnlyList<TermView>>
{
    private readonly IAppDbContext _db;

    public ListTermsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<TermView>> Handle(
        ListTermsQuery request,
        CancellationToken cancellationToken) =>
        await _db.Terms
            .AsNoTracking()
            .OrderByDescending(item => item.StartDate)
            .Select(item => new TermView(
                item.Id,
                item.Name,
                item.StartDate,
                item.EndDate,
                item.Status))
            .ToListAsync(cancellationToken);
}

public sealed record SaveTermCommand(
    Guid? Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status) : IRequest<TermResult>;

public sealed record TermResult(Guid Id);

public sealed class SaveTermCommandValidator : AbstractValidator<SaveTermCommand>
{
    private static readonly string[] Statuses = ["Upcoming", "Active", "Completed"];

    public SaveTermCommandValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(128);
        RuleFor(item => item.EndDate)
            .GreaterThan(item => item.StartDate)
            .WithMessage("The term end date must be after its start date.");
        RuleFor(item => item.Status)
            .Must(status => Statuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be Upcoming, Active, or Completed.");
    }
}

public sealed class SaveTermCommandHandler : IRequestHandler<SaveTermCommand, TermResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SaveTermCommandHandler(
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

    public async Task<TermResult> Handle(
        SaveTermCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var status = CanonicalStatus(request.Status);
        var duplicateName = await _db.Terms.AnyAsync(
            item => item.Id != request.Id && item.Name == name,
            cancellationToken);
        if (duplicateName)
        {
            throw new ConflictException("A term with this name already exists.");
        }
        if (status == "Active"
            && (request.StartDate > _clock.UtcNow || request.EndDate < _clock.UtcNow))
        {
            throw new ConflictException("An active term must include today's date.");
        }
        if (status == "Active" && await _db.Terms.AnyAsync(
            item => item.Id != request.Id && item.Status == "Active",
            cancellationToken))
        {
            throw new ConflictException("Complete the current active term before activating another.");
        }

        var created = request.Id is null;
        var term = request.Id is { } id
            ? await _db.Terms.FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
                ?? throw new NotFoundException("Term not found.")
            : new Term();
        if (created)
        {
            _db.Terms.Add(term);
        }

        term.Name = name;
        term.StartDate = request.StartDate;
        term.EndDate = request.EndDate;
        term.Status = status;
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            created ? "TermCreated" : "TermUpdated",
            _currentUser.UserId,
            "Term",
            term.Id,
            cancellationToken: cancellationToken);
        return new TermResult(term.Id);
    }

    private static string CanonicalStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "active" => "Active",
            "completed" => "Completed",
            _ => "Upcoming",
        };
}
