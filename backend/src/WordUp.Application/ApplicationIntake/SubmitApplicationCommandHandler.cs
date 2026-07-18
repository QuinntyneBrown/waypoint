using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, SubmitApplicationResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public SubmitApplicationCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
    }

    public async Task<SubmitApplicationResult> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
    {
        // Intake is anonymous: an application row is created but NO user account (L2-INTAKE-001 AC4).
        var application = new YouthApplication
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Age = request.Age,
            Neighbourhood = request.Neighbourhood.Trim(),
            WhyInterested = request.WhyInterested.Trim(),
            TrackInterest = request.TrackInterest?.Trim(),
            GuardianName = request.GuardianName?.Trim(),
            GuardianEmail = request.GuardianEmail?.Trim().ToLowerInvariant(),
            GuardianPhone = request.GuardianPhone?.Trim(),
            ParticipationConsentAck = request.ParticipationConsentAck,
            PrivacyAck = request.PrivacyAck,
            Status = ApplicationStatus.New,
            CreatedAt = _clock.UtcNow,
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("ApplicationSubmitted", null, "YouthApplication", application.Id);

        return new SubmitApplicationResult(application.Id);
    }
}
