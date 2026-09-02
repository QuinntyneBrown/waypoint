using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;
using Waypoint.Application.ConsentAndSafeguarding;

namespace Waypoint.Application.ApplicationIntake;

public class ApproveApplicationCommandHandler : IRequestHandler<ApproveApplicationCommand, ApproveApplicationResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailSender _email;

    public ApproveApplicationCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IEmailSender email)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task<ApproveApplicationResult> Handle(ApproveApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");
        var now = _clock.UtcNow;
        if (application.Status is ApplicationStatus.Approved or ApplicationStatus.Declined)
            throw new ConflictException("This application already has a final decision.");

        var cohort = await _db.Cohorts.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CohortId, cancellationToken)
            ?? throw new ConflictException("Choose an available cohort.");
        if (cohort.TrackId is { } cohortTrackId
            && !await _db.Tracks.AsNoTracking().AnyAsync(
                track => track.Id == cohortTrackId
                    && track.Enabled
                    && track.Status == PublishStatus.Published,
                cancellationToken))
        {
            throw new ConflictException("The selected cohort's track is paused.");
        }
        var mentor = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.MentorId
                && item.Role == Roles.Mentor
                && item.Status == UserStatus.Active,
                cancellationToken)
            ?? throw new ConflictException("Choose an active mentor.");
        var mentorBelongsToCohort = cohort.MentorId == mentor.Id
            || await _db.CohortMentors.AsNoTracking().AnyAsync(
                item => item.CohortId == cohort.Id && item.MentorId == mentor.Id,
                cancellationToken);
        if (!mentorBelongsToCohort)
            throw new ConflictException("The selected mentor is not assigned to this cohort.");

        var policy = await ConsentPolicyEvaluator.LoadAsync(_db, now, cancellationToken);
        var consentRows = application.CreatedYouthUserId is { } consentYouthId
            ? await _db.GuardianConsents.AsNoTracking()
                .Where(consent => consent.YouthUserId == consentYouthId)
                .ToListAsync(cancellationToken)
            : [];
        var hasValidConsent = consentRows.Any(consent =>
            policy.IsParticipationValid(consent, now));
        if (application.IsMinor && !hasValidConsent)
            throw new ConflictException(
                "Guardian participation consent must be recorded before approval.");

        // Provision the Youth account and enroll into the chosen cohort + mentor (L2-INTAKE-002 AC3).
        // No account existed prior to approval; there is no password yet — invite/onboarding sets it.
        var email = !string.IsNullOrWhiteSpace(application.GuardianEmail)
            ? application.GuardianEmail!.Trim().ToLowerInvariant()
            : $"youth.{application.Id:N}@applicants.waypoint.local";

        // Guard against an email collision (e.g. a guardian who is also a user).
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            email = $"youth.{application.Id:N}@applicants.waypoint.local";
        }

        var youth = application.CreatedYouthUserId is { } youthId
            ? await _db.Users.FirstOrDefaultAsync(
                item => item.Id == youthId && item.Role == Roles.Youth,
                cancellationToken)
                ?? throw new ConflictException("The linked youth account is unavailable.")
            : new User
            {
                Email = email,
                PasswordHash = string.Empty,
                Role = Roles.Youth,
                CreatedAt = now,
            };
        youth.DisplayName = $"{application.FirstName} {application.LastName}".Trim();
        youth.Status = UserStatus.Active;
        youth.CohortId = request.CohortId;
        youth.MentorId = request.MentorId;
        youth.OnboardingCompleted = false;
        if (application.CreatedYouthUserId is null)
        {
            _db.Users.Add(youth);
        }

        if (cohort.TrackId is { } trackId
            && !await _db.YouthTrackEnrollments.AnyAsync(
                item => item.YouthUserId == youth.Id && item.TrackId == trackId,
                cancellationToken))
        {
            _db.YouthTrackEnrollments.Add(new YouthTrackEnrollment
            {
                YouthUserId = youth.Id,
                TrackId = trackId,
                EnrolledAt = now,
            });
        }

        application.Status = ApplicationStatus.Approved;
        application.CohortId = request.CohortId;
        application.MentorId = request.MentorId;
        application.TrackInterest = request.TrackInterest?.Trim() ?? application.TrackInterest;
        application.CreatedYouthUserId = youth.Id;
        application.DecisionNote = request.DecisionNote?.Trim();
        application.ReviewedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ApplicationApproved",
            _currentUser.UserId,
            "YouthApplication",
            application.Id,
            cancellationToken: cancellationToken);
        var decisionRecipient = await ApplicationDecisionEmailRecipientResolver.ResolveAsync(
            _db,
            application,
            cancellationToken);
        if (decisionRecipient is not null)
        {
            await _email.SendAsync(
                decisionRecipient.Email,
                "Your Waypoint application was approved",
                $"{youth.DisplayName} has been approved for {cohort.Name} with {mentor.DisplayName}. " +
                "Waypoint staff will follow up with account access details.",
                cancellationToken,
                subjectUserId: youth.Id,
                recipientUserId: decisionRecipient.UserId);
        }

        return new ApproveApplicationResult(youth.Id, "Approved");
    }
}
