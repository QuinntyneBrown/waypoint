using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.Competencies;

public class SetCompetencyLevelCommandHandler : IRequestHandler<SetCompetencyLevelCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public SetCompetencyLevelCommandHandler(
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

    public async Task Handle(SetCompetencyLevelCommand request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");

        // No-IDOR: a mentor may only assess a youth assigned to them; an admin may assess any youth (L2-AUTH-010).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var competencyExists = await _db.Competencies
            .AnyAsync(c => c.Id == request.CompetencyId, cancellationToken);
        if (!competencyExists)
        {
            throw new NotFoundException("Competency not found.");
        }

        var previousAt = await _db.CompetencyAssessments
            .Where(assessment => assessment.YouthUserId == request.YouthId
                && assessment.CompetencyId == request.CompetencyId)
            .MaxAsync(assessment => (DateTime?)assessment.AssessedAt, cancellationToken);
        var clockNow = _clock.UtcNow;
        var now = previousAt >= clockNow
            ? previousAt.Value.AddTicks(1)
            : clockNow;

        // Append assessments so the latest value drives views without erasing the audit history.
        var assessment = new CompetencyAssessment
        {
            YouthUserId = request.YouthId,
            CompetencyId = request.CompetencyId,
            Level = request.Level,
            Notes = request.Notes,
            AssessedByUserId = _currentUser.UserId!.Value,
            AssessedAt = now,
        };
        _db.CompetencyAssessments.Add(assessment);

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "CompetencyLevelSet",
            _currentUser.UserId,
            "CompetencyAssessment",
            assessment.Id,
            details: $"competency={request.CompetencyId};level={request.Level}",
            cancellationToken: cancellationToken);
    }
}
