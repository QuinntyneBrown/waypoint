using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.PlatformServices.Privacy;

public record FulfillDataSubjectRequestCommand(
    Guid Id,
    string? DisplayName,
    string? Email,
    string? Pronouns,
    bool? ClearPronouns,
    string? ResolutionNote,
    bool VerifiedOwnershipAttested = false,
    IReadOnlyList<Guid>? VerifiedContactRequestIds = null,
    IReadOnlyList<Guid>? VerifiedVolunteerSignupIds = null,
    IReadOnlyList<Guid>? VerifiedDonationPledgeIds = null,
    IReadOnlyList<Guid>? VerifiedApplicantApplicationIds = null,
    IReadOnlyList<Guid>? VerifiedGuardianApplicationIds = null)
    : IRequest<DataSubjectRequestResult>, IAtomicOutboxRequest;

public sealed class FulfillDataSubjectRequestCommandValidator
    : AbstractValidator<FulfillDataSubjectRequestCommand>
{
    public FulfillDataSubjectRequestCommandValidator()
    {
        RuleFor(item => item.DisplayName).MaximumLength(128);
        RuleFor(item => item.Email).EmailAddress().MaximumLength(256)
            .When(item => !string.IsNullOrWhiteSpace(item.Email));
        RuleFor(item => item.Pronouns).MaximumLength(64);
        RuleFor(item => item).Must(command =>
                command.ClearPronouns != true || string.IsNullOrWhiteSpace(command.Pronouns))
            .WithMessage("Pronouns cannot be supplied when ClearPronouns is true.");
        RuleFor(item => item.ResolutionNote).MaximumLength(4000);
        RuleFor(item => item).Must(item => VerifiedLinkCount(item) <= 200)
            .WithMessage("No more than 200 verified records can be attached at once.");
        RuleFor(item => item.VerifiedOwnershipAttested).Equal(true)
            .When(item => VerifiedLinkCount(item) > 0)
            .WithMessage("Verified ownership must be explicitly attested before records are linked.");
        RuleForEach(item => item.VerifiedContactRequestIds).NotEqual(Guid.Empty);
        RuleForEach(item => item.VerifiedVolunteerSignupIds).NotEqual(Guid.Empty);
        RuleForEach(item => item.VerifiedDonationPledgeIds).NotEqual(Guid.Empty);
        RuleForEach(item => item.VerifiedApplicantApplicationIds).NotEqual(Guid.Empty);
        RuleForEach(item => item.VerifiedGuardianApplicationIds).NotEqual(Guid.Empty);
    }

    private static int VerifiedLinkCount(FulfillDataSubjectRequestCommand item) =>
        (item.VerifiedContactRequestIds?.Distinct().Count() ?? 0)
        + (item.VerifiedVolunteerSignupIds?.Distinct().Count() ?? 0)
        + (item.VerifiedDonationPledgeIds?.Distinct().Count() ?? 0)
        + (item.VerifiedApplicantApplicationIds?.Distinct().Count() ?? 0)
        + (item.VerifiedGuardianApplicationIds?.Distinct().Count() ?? 0);
}

public sealed class FulfillDataSubjectRequestCommandHandler
    : IRequestHandler<FulfillDataSubjectRequestCommand, DataSubjectRequestResult>
{
    // The database unique index remains authoritative across application instances. This small
    // in-process gate also makes the check-and-write deterministic for the non-relational test and
    // development provider, which does not enforce unique indexes.
    private static readonly SemaphoreSlim EmailCorrectionGate = new(1, 1);

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly PersonalDataAnonymizer _anonymizer;
    private readonly IEmailSender _email;
    private readonly IApplicationLinkBuilder _links;
    private readonly IDatabaseErrorClassifier _databaseErrors;
    private readonly IUserPrivacyLock _privacyLock;
    private readonly IVerifiedRecordOwnershipClaimer _ownershipClaimer;

    public FulfillDataSubjectRequestCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit,
        PersonalDataAnonymizer anonymizer,
        IEmailSender email,
        IApplicationLinkBuilder links,
        IDatabaseErrorClassifier databaseErrors,
        IUserPrivacyLock privacyLock,
        IVerifiedRecordOwnershipClaimer ownershipClaimer)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
        _anonymizer = anonymizer;
        _email = email;
        _links = links;
        _databaseErrors = databaseErrors;
        _privacyLock = privacyLock;
        _ownershipClaimer = ownershipClaimer;
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

        // SQL Server owns this lock through the surrounding IAtomicRequest transaction. Delivery
        // producers and workers for this account cannot cross the correction/removal commit.
        await _privacyLock.AcquireAsync(request.SubjectUserId, cancellationToken);
        var subject = await _db.Users.FirstOrDefaultAsync(
            item => item.Id == request.SubjectUserId,
            cancellationToken) ?? throw new NotFoundException("Data subject not found.");
        var isAlreadyAnonymized = subject.PersonalDataAnonymizedAt is not null;
        var isRemovedWithoutErasure = subject.Status == UserStatus.Removed && !isAlreadyAnonymized;
        if (isAlreadyAnonymized
            || isRemovedWithoutErasure && request.ActionType != "removal")
        {
            throw new ConflictException(
                "This data subject has already been removed and cannot be changed or exported.");
        }
        var originalEmail = subject.Email;
        var now = _clock.UtcNow;
        var verifiedLinkCount = await AttachVerifiedRecordsAsync(
            subject.Id,
            command,
            cancellationToken);
        if (verifiedLinkCount > 0)
        {
            // The subsequent correction/removal queries must observe these links in this same
            // transaction. Rollback still removes them if fulfillment fails.
            await _db.SaveChangesAsync(cancellationToken);
        }
        var serializeEmailCorrection = request.ActionType == "correction"
            && !string.IsNullOrWhiteSpace(command.Email ?? request.RequestedEmail);
        if (serializeEmailCorrection)
        {
            await EmailCorrectionGate.WaitAsync(cancellationToken);
        }
        try
        {
            switch (request.ActionType)
            {
                case "access":
                    break;
                case "correction":
                    await ApplyCorrectionAsync(subject, request, command, cancellationToken);
                    // Pending bodies can contain arbitrary snapshots of the subject's old profile.
                    // They cannot be safely rewritten after a correction, so discard those addressed
                    // to anyone else while the subject lock excludes claimed SMTP delivery. Envelopes
                    // addressed to the subject (recipient-only or about themselves) disclose nothing
                    // to a third party; they remain eligible and email corrections readdress them.
                    var staleSubjectEmails = await _db.OutboundEmails
                        .Where(item => item.SubjectUserId == subject.Id
                            && item.SentAt == null
                            && (item.RecipientUserId == null || item.RecipientUserId != subject.Id))
                        .ToListAsync(cancellationToken);
                    _db.OutboundEmails.RemoveRange(staleSubjectEmails);
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
            request.ResolutionNote = request.ActionType == "removal"
                ? null
                : string.IsNullOrWhiteSpace(command.ResolutionNote)
                    ? request.ActionType == "access"
                        ? "Your comprehensive personal-data export is ready in your signed-in privacy settings."
                        : null
                    : WebUtility.HtmlEncode(command.ResolutionNote.Trim());
            request.ConcurrencyStamp = Guid.NewGuid();
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("This data-subject request has already been fulfilled.");
            }
            catch (DbUpdateException exception) when (
                _databaseErrors.IsUniqueConstraintViolation(exception, "IX_Users_Email"))
            {
                throw new ConflictException("That email address is already in use.");
            }
        }
        finally
        {
            if (serializeEmailCorrection)
            {
                EmailCorrectionGate.Release();
            }
        }
        await _audit.LogAsync(
            $"DataSubjectRequestFulfilled:{request.ActionType}",
            actorId,
            "DataSubjectRequest",
            request.Id,
            details: $"ref={request.Reference}; subject={request.SubjectUserId}; verifiedLinks={verifiedLinkCount}",
            cancellationToken: cancellationToken);
        var completionEmail = request.ActionType == "removal" ? originalEmail : subject.Email;
        await _email.SendAsync(
            completionEmail,
            "Your Waypoint privacy request is complete",
            request.ActionType == "access"
                ? $"Privacy request {request.Reference} has been completed. Sign in to download your comprehensive personal-data export: {_links.Portal("/settings/privacy")}"
                : $"Privacy request {request.Reference} has been completed. Review your request history: {_links.Portal("/settings/privacy")}",
            cancellationToken,
            $"data-request-completed:{request.Id:N}",
            subjectUserId: subject.Id,
            purgeAfterDelivery: request.ActionType == "removal",
            recipientUserId: request.ActionType == "removal" ? null : subject.Id,
            isPrivacyRemovalConfirmation: request.ActionType == "removal");

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
        DataSubjectRequest request,
        FulfillDataSubjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        var displayName = command.DisplayName ?? request.RequestedDisplayName;
        var emailAddress = command.Email ?? request.RequestedEmail;
        var pronounChange = ResolvePronounChange(command, request);
        if (string.IsNullOrWhiteSpace(displayName)
            && string.IsNullOrWhiteSpace(emailAddress)
            && !pronounChange.Apply)
        {
            throw new ConflictException("A correction must change at least one profile field.");
        }

        if (!string.IsNullOrWhiteSpace(emailAddress))
        {
            var email = emailAddress.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(
                item => item.Email == email && item.Id != subject.Id,
                cancellationToken))
            {
                throw new ConflictException("That email address is already in use.");
            }
            if (!string.Equals(subject.Email, email, StringComparison.Ordinal))
            {
                await MigrateEmailLinkedRecordsAsync(
                    subject.Id,
                    subject.Email,
                    email,
                    cancellationToken);
                subject.Email = email;
            }
        }
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            subject.DisplayName = displayName.Trim();
        }
        if (pronounChange.Apply)
        {
            subject.Pronouns = pronounChange.Value;
        }
    }

    private async Task<int> AttachVerifiedRecordsAsync(
        Guid subjectUserId,
        FulfillDataSubjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        var contactIds = DistinctIds(command.VerifiedContactRequestIds);
        var volunteerIds = DistinctIds(command.VerifiedVolunteerSignupIds);
        var donationIds = DistinctIds(command.VerifiedDonationPledgeIds);
        var applicantIds = DistinctIds(command.VerifiedApplicantApplicationIds);
        var guardianIds = DistinctIds(command.VerifiedGuardianApplicationIds);
        if (applicantIds.Intersect(guardianIds).Any())
        {
            throw new ConflictException(
                "An application cannot be verified as both applicant-owned and guardian-owned in one fulfillment.");
        }

        return await _ownershipClaimer.ClaimAsync(
            subjectUserId,
            contactIds,
            volunteerIds,
            donationIds,
            applicantIds,
            guardianIds,
            cancellationToken);
    }

    private static List<Guid> DistinctIds(IReadOnlyList<Guid>? ids) =>
        ids?.Distinct().OrderBy(item => item).ToList() ?? [];

    private static (bool Apply, string? Value) ResolvePronounChange(
        FulfillDataSubjectRequestCommand command,
        DataSubjectRequest request)
    {
        if (!string.IsNullOrWhiteSpace(command.Pronouns))
        {
            return (true, command.Pronouns.Trim());
        }
        if (command.ClearPronouns == true)
        {
            return (true, null);
        }
        if (command.ClearPronouns == false)
        {
            return (false, null);
        }
        if (!string.IsNullOrWhiteSpace(request.RequestedPronouns))
        {
            return (true, request.RequestedPronouns.Trim());
        }
        return request.RequestedClearPronouns ? (true, null) : (false, null);
    }

    private async Task MigrateEmailLinkedRecordsAsync(
        Guid subjectUserId,
        string oldEmail,
        string newEmail,
        CancellationToken cancellationToken)
    {
        // Explicit ownership links drive updates to encrypted address columns. Invitations use a
        // normalized, indexed plaintext address and can be updated with a bounded SQL predicate.
        var applications = await _db.Applications
            .Where(item => item.GuardianUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in applications)
            item.GuardianEmail = newEmail;

        var contacts = await _db.ContactRequests
            .Where(item => item.OwnerUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in contacts)
            item.Email = newEmail;

        var volunteers = await _db.VolunteerSignups
            .Where(item => item.OwnerUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in volunteers)
            item.Email = newEmail;

        var donations = await _db.DonationPledges
            .Where(item => item.OwnerUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in donations)
            item.Email = newEmail;

        var outboundEmails = await _db.OutboundEmails
            .Where(item => item.RecipientUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in outboundEmails)
            item.ToEmail = newEmail;

        var consentRequests = await _db.GuardianConsentRequests
            .Where(item => item.GuardianUserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in consentRequests)
            item.Destination = newEmail;

        var supportRequests = await _db.SupportRequests
            .Where(item => item.UserId == subjectUserId)
            .ToListAsync(cancellationToken);
        foreach (var item in supportRequests)
            item.ReplyToEmail = newEmail;

        var invitations = await _db.Invitations
            .Where(item => item.Email == oldEmail)
            .ToListAsync(cancellationToken);
        foreach (var item in invitations)
            item.Email = newEmail;
    }
}
