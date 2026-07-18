using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ApplicationIntake;

public class ApproveApplicationCommandHandler : IRequestHandler<ApproveApplicationCommand, ApproveApplicationResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ApproveApplicationCommandHandler(
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

    public async Task<ApproveApplicationResult> Handle(ApproveApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        var now = _clock.UtcNow;

        // Provision the Youth account and enroll into the chosen cohort + mentor (L2-INTAKE-002 AC3).
        // No account existed prior to approval; there is no password yet — invite/onboarding sets it.
        var email = !string.IsNullOrWhiteSpace(application.GuardianEmail)
            ? application.GuardianEmail!.Trim().ToLowerInvariant()
            : $"youth.{application.Id:N}@applicants.wordup.local";

        // Guard against an email collision (e.g. a guardian who is also a user).
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            email = $"youth.{application.Id:N}@applicants.wordup.local";
        }

        var youth = new User
        {
            Email = email,
            PasswordHash = string.Empty,
            DisplayName = $"{application.FirstName} {application.LastName}".Trim(),
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = request.CohortId,
            MentorId = request.MentorId,
            OnboardingCompleted = false,
            CreatedAt = now,
        };
        _db.Users.Add(youth);

        // A minor is still consent-gated downstream by the CONSENT subsystem; approval is not blocked here.
        application.Status = ApplicationStatus.Approved;
        application.CohortId = request.CohortId;
        application.MentorId = request.MentorId;
        application.TrackInterest = request.TrackInterest?.Trim() ?? application.TrackInterest;
        application.CreatedYouthUserId = youth.Id;
        application.DecisionNote = request.DecisionNote?.Trim();
        application.ReviewedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("ApplicationApproved", _currentUser.UserId, "YouthApplication", application.Id);

        return new ApproveApplicationResult(youth.Id);
    }
}
