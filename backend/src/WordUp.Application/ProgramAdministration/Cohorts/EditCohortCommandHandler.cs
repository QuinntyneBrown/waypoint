using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public class EditCohortCommandHandler : IRequestHandler<EditCohortCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public EditCohortCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(EditCohortCommand request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");

        cohort.Name = request.Name.Trim();

        cohort.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();

        if (request.LeadMentorId is { } leadMentorId)
        {
            var mentorExists = await _db.Users.AnyAsync(
                user => user.Id == leadMentorId
                    && user.Role == Roles.Mentor
                    && user.Status == UserStatus.Active,
                cancellationToken);
            if (!mentorExists)
            {
                throw new ConflictException("Choose an active mentor as cohort lead.");
            }

            cohort.MentorId = leadMentorId;

            // Keep the lead CohortMentor row in step with the cohort's lead mentor (L2-ADMIN-003 AC3).
            // A supporting mentor promoted through this edit path must not leave a duplicate row.
            var promotedSupportingMentor = await _db.CohortMentors
                .FirstOrDefaultAsync(
                    mentor => mentor.CohortId == cohort.Id
                        && mentor.MentorId == leadMentorId
                        && !mentor.IsLead,
                    cancellationToken);
            if (promotedSupportingMentor is not null)
            {
                _db.CohortMentors.Remove(promotedSupportingMentor);
            }
            var existingLead = await _db.CohortMentors
                .FirstOrDefaultAsync(m => m.CohortId == cohort.Id && m.IsLead, cancellationToken);

            if (existingLead is null)
            {
                _db.CohortMentors.Add(new CohortMentor
                {
                    CohortId = cohort.Id,
                    MentorId = leadMentorId,
                    IsLead = true,
                });
            }
            else
            {
                existingLead.MentorId = leadMentorId;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CohortUpdated", _currentUser.UserId, "Cohort", cohort.Id);
    }
}
