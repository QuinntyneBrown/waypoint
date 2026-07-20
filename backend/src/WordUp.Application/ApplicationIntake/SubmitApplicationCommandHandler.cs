using MediatR;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Security;

namespace WordUp.Application.ApplicationIntake;

public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, SubmitApplicationResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly AuthenticatedPrivacyWriteGuard _privacyWriteGuard;

    public SubmitApplicationCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        AuthenticatedPrivacyWriteGuard privacyWriteGuard)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _privacyWriteGuard = privacyWriteGuard;
    }

    public async Task<SubmitApplicationResult> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
    {
        // Intake never creates a user account (L2-INTAKE-001 AC4). If the author already has an
        // authenticated account, serialize the explicit ownership link with account removal.
        var whoIsFilling = request.WhoIsFilling.Trim().ToLowerInvariant();
        var authenticatedAuthorId = await _privacyWriteGuard.AcquireAuthorAsync(cancellationToken);
        var application = new YouthApplication
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Age = request.Age,
            WhoIsFilling = whoIsFilling,
            Neighbourhood = request.Neighbourhood.Trim(),
            CohortInterest = request.CohortInterest.Trim(),
            WhyInterested = request.WhyInterested.Trim(),
            TrackInterest = request.TrackInterest?.Trim(),
            GuardianName = request.GuardianName?.Trim(),
            GuardianEmail = request.GuardianEmail?.Trim().ToLowerInvariant(),
            GuardianPhone = request.GuardianPhone?.Trim(),
            ParticipationConsentAck = request.ParticipationConsentAck,
            PrivacyAck = request.PrivacyAck,
            Status = ApplicationStatus.New,
            ApplicantUserId = whoIsFilling == "self" ? authenticatedAuthorId : null,
            GuardianUserId = whoIsFilling == "guardian" ? authenticatedAuthorId : null,
            CreatedAt = _clock.UtcNow,
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "ApplicationSubmitted",
            authenticatedAuthorId,
            "YouthApplication",
            application.Id);

        return new SubmitApplicationResult(application.Id);
    }
}
