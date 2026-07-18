using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ApplicationIntake;

public sealed class ApplicationDetailQueryHandler
    : IRequestHandler<ApplicationDetailQuery, ApplicationDetailView>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public ApplicationDetailQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ApplicationDetailView> Handle(
        ApplicationDetailQuery request,
        CancellationToken cancellationToken)
    {
        var application = await _db.Applications.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");
        var hasValidConsent = application.CreatedYouthUserId is { } youthId
            && await _db.GuardianConsents.AsNoTracking().AnyAsync(
                consent => consent.YouthUserId == youthId
                    && consent.ParticipationConsent
                    && consent.WithdrawnAt == null
                    && consent.ValidFrom <= _clock.UtcNow
                    && consent.ValidUntil >= _clock.UtcNow,
                cancellationToken);

        return new ApplicationDetailView(
            application.Id,
            application.FirstName,
            application.LastName,
            application.Age,
            application.IsMinor,
            application.WhoIsFilling,
            application.Neighbourhood,
            application.CohortInterest,
            application.WhyInterested,
            application.TrackInterest,
            application.GuardianName,
            application.GuardianEmail,
            application.GuardianPhone,
            application.ParticipationConsentAck,
            application.PrivacyAck,
            ApplicationReviewState.EffectiveStatus(application, hasValidConsent),
            ApplicationReviewState.ConsentState(application, hasValidConsent),
            ApplicationReviewState.CanApprove(application, hasValidConsent),
            application.CohortId,
            application.MentorId,
            application.DecisionNote,
            application.CreatedAt,
            application.ReviewedAt);
    }
}
