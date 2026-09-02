using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.MentorAssignments;

public sealed class SetCohortMentorsCommandHandler : IRequestHandler<SetCohortMentorsCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SetCohortMentorsCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(SetCohortMentorsCommand request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts
            .FirstOrDefaultAsync(item => item.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");
        var supportingIds = request.SupportingMentorIds.Distinct().ToList();
        if (supportingIds.Count != request.SupportingMentorIds.Count)
        {
            throw new ConflictException("Choose each supporting mentor only once.");
        }
        if (supportingIds.Contains(request.LeadMentorId))
        {
            throw new ConflictException("The lead mentor cannot also be a supporting mentor.");
        }

        var selectedIds = supportingIds.Append(request.LeadMentorId).ToList();
        var activeMentorCount = await _db.Users.CountAsync(
            item => selectedIds.Contains(item.Id)
                && item.Role == Roles.Mentor
                && item.Status == UserStatus.Active,
            cancellationToken);
        if (activeMentorCount != selectedIds.Count)
        {
            throw new ConflictException("Choose active mentors for the cohort team.");
        }

        cohort.MentorId = request.LeadMentorId;
        var existing = await _db.CohortMentors
            .Where(item => item.CohortId == cohort.Id)
            .ToListAsync(cancellationToken);
        _db.CohortMentors.RemoveRange(existing);
        _db.CohortMentors.Add(new CohortMentor
        {
            CohortId = cohort.Id,
            MentorId = request.LeadMentorId,
            IsLead = true,
        });
        foreach (var mentorId in supportingIds)
        {
            _db.CohortMentors.Add(new CohortMentor
            {
                CohortId = cohort.Id,
                MentorId = mentorId,
                IsLead = false,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "CohortMentorsAssigned",
            _currentUser.UserId,
            "Cohort",
            cohort.Id,
            details: $"lead={request.LeadMentorId};supporting={string.Join(',', supportingIds)}",
            cancellationToken: cancellationToken);
    }
}
