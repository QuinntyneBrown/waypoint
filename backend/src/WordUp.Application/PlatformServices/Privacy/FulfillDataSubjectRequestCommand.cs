using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.PlatformServices.Privacy;

public record FulfillDataSubjectRequestCommand(
    Guid Id,
    string? DisplayName,
    string? Email,
    string? Pronouns,
    string? ResolutionNote) : IRequest<DataSubjectRequestResult>;

public sealed class FulfillDataSubjectRequestCommandValidator
    : AbstractValidator<FulfillDataSubjectRequestCommand>
{
    public FulfillDataSubjectRequestCommandValidator()
    {
        RuleFor(item => item.DisplayName).MaximumLength(128);
        RuleFor(item => item.Email).EmailAddress().MaximumLength(256)
            .When(item => !string.IsNullOrWhiteSpace(item.Email));
        RuleFor(item => item.Pronouns).MaximumLength(64);
        RuleFor(item => item.ResolutionNote).MaximumLength(4000);
    }
}

public sealed class FulfillDataSubjectRequestCommandHandler
    : IRequestHandler<FulfillDataSubjectRequestCommand, DataSubjectRequestResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly PersonalDataAnonymizer _anonymizer;

    public FulfillDataSubjectRequestCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit,
        PersonalDataAnonymizer anonymizer)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
        _anonymizer = anonymizer;
    }

    public async Task<DataSubjectRequestResult> Handle(
        FulfillDataSubjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? throw new ForbiddenException();
        if (_currentUser.Role != Roles.ProgramAdministrator)
        {
            throw new ForbiddenException();
        }

        var request = await _db.DataSubjectRequests
            .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Data-subject request not found.");
        if (request.Status != DataSubjectRequestStatus.Pending)
        {
            throw new ConflictException("This data-subject request has already been fulfilled.");
        }
        if (request.SubjectUserId == actorId && request.ActionType == "removal")
        {
            throw new ForbiddenException("Another administrator must approve your removal request.");
        }

        var subject = await _db.Users.FirstOrDefaultAsync(
            item => item.Id == request.SubjectUserId,
            cancellationToken) ?? throw new NotFoundException("Data subject not found.");
        var now = _clock.UtcNow;
        switch (request.ActionType)
        {
            case "access":
                break;
            case "correction":
                await ApplyCorrectionAsync(subject, command, cancellationToken);
                break;
            case "removal":
                await _anonymizer.AnonymizeAsync(subject, now, cancellationToken);
                break;
            case "consent-withdrawal":
                var consents = await _db.GuardianConsents
                    .Where(item => (item.YouthUserId == subject.Id || item.GuardianUserId == subject.Id)
                        && item.WithdrawnAt == null)
                    .ToListAsync(cancellationToken);
                foreach (var consent in consents) consent.WithdrawnAt = now;
                break;
            default:
                throw new ConflictException("This data-subject request type cannot be fulfilled.");
        }

        request.Status = DataSubjectRequestStatus.Completed;
        request.CompletedByUserId = actorId;
        request.CompletedAt = now;
        request.ResolutionNote = string.IsNullOrWhiteSpace(command.ResolutionNote)
            ? null
            : WebUtility.HtmlEncode(command.ResolutionNote.Trim());
        await _audit.LogAsync(
            $"DataSubjectRequestFulfilled:{request.ActionType}",
            actorId,
            "DataSubjectRequest",
            request.Id,
            details: $"ref={request.Reference}; subject={request.SubjectUserId}",
            cancellationToken: cancellationToken);

        return new DataSubjectRequestResult(
            request.Id,
            request.Reference,
            request.SubjectUserId,
            request.ActionType,
            request.Status.ToString(),
            request.RequestedAt);
    }

    private async Task ApplyCorrectionAsync(
        User subject,
        FulfillDataSubjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.DisplayName)
            && string.IsNullOrWhiteSpace(command.Email)
            && command.Pronouns is null)
        {
            throw new ConflictException("A correction must change at least one profile field.");
        }

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var email = command.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(
                item => item.Email == email && item.Id != subject.Id,
                cancellationToken))
            {
                throw new ConflictException("That email address is already in use.");
            }
            subject.Email = email;
        }
        if (!string.IsNullOrWhiteSpace(command.DisplayName))
        {
            subject.DisplayName = command.DisplayName.Trim();
        }
        if (command.Pronouns is not null)
        {
            subject.Pronouns = string.IsNullOrWhiteSpace(command.Pronouns)
                ? null
                : command.Pronouns.Trim();
        }
    }
}
