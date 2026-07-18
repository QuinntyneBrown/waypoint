using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Competencies;

public class YouthCompetenciesQueryHandler : IRequestHandler<YouthCompetenciesQuery, CompetencySummary>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public YouthCompetenciesQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CompetencySummary> Handle(YouthCompetenciesQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // No-IDOR: a mentor may only view a youth assigned to them; an admin may view any youth (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        // Latest assessment per competency for the youth (MENTOR-005 AC2/AC3).
        var assessments = await _db.CompetencyAssessments
            .Where(a => a.YouthUserId == request.YouthId)
            .ToListAsync(cancellationToken);

        var latestPerCompetency = assessments
            .GroupBy(a => a.CompetencyId)
            .Select(g => g.OrderByDescending(a => a.AssessedAt).First())
            .OrderBy(a => a.AssessedAt)
            .ToList();

        var items = latestPerCompetency
            .Select(a => new CompetencyLevelDto(a.CompetencyId, a.Level.ToString(), a.Notes))
            .ToList();

        var masteredOrProficient = latestPerCompetency
            .Count(a => a.Level == CompetencyLevel.Proficient || a.Level == CompetencyLevel.Mastered);

        return new CompetencySummary(items, masteredOrProficient);
    }
}
