using MediatR;
using WordUp.Application.Abstractions;
using WordUp.Domain;

namespace WordUp.Application.Cohorts;

public class CreateCohortCommandHandler : IRequestHandler<CreateCohortCommand, CohortResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateCohortCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<CohortResult> Handle(CreateCohortCommand request, CancellationToken cancellationToken)
    {
        var cohort = new Cohort
        {
            Name = request.Name.Trim(),
            TrackId = request.TrackId,
            MentorId = request.LeadMentorId,
            TermStart = request.StartDate ?? default,
            TermEnd = request.EndDate ?? default,
            Location = request.Location,
        };

        _db.Cohorts.Add(cohort);
        _db.CohortMentors.Add(new CohortMentor
        {
            CohortId = cohort.Id,
            MentorId = request.LeadMentorId,
            IsLead = true,
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CohortCreated", _currentUser.UserId, "Cohort", cohort.Id);

        return new CohortResult(cohort.Id);
    }
}
