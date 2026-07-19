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

        var stages = await _db.ProgressionStages
            .OrderBy(stage => stage.Order)
            .ThenBy(stage => stage.Name)
            .ToListAsync(cancellationToken);
        var stageById = stages.ToDictionary(stage => stage.Id);
        var competencies = await _db.Competencies
            .Where(item => item.ArchivedAt == null)
            .ToListAsync(cancellationToken);
        competencies = competencies
            .OrderBy(competency => stageById.GetValueOrDefault(competency.StageId)?.Order ?? int.MaxValue)
            .ThenBy(competency => competency.Order)
            .ThenBy(competency => competency.Name)
            .ToList();

        // Preserve history and render the latest assessment per competency (MENTOR-005 AC2/AC3).
        var assessments = await _db.CompetencyAssessments
            .Where(a => a.YouthUserId == request.YouthId)
            .ToListAsync(cancellationToken);

        var latestByCompetency = assessments
            .GroupBy(a => a.CompetencyId)
            .Select(g => g.OrderByDescending(a => a.AssessedAt).First())
            .ToDictionary(assessment => assessment.CompetencyId);
        var assessorIds = latestByCompetency.Values
            .Select(assessment => assessment.AssessedByUserId)
            .Distinct()
            .ToList();
        var assessorNames = await _db.Users
            .Where(user => assessorIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);

        var items = competencies
            .Select(competency =>
            {
                latestByCompetency.TryGetValue(competency.Id, out var assessment);
                return new CompetencyLevelDto(
                    competency.Id,
                    competency.Name,
                    stageById.GetValueOrDefault(competency.StageId)?.Name ?? "Competencies",
                    (assessment?.Level ?? CompetencyLevel.NotStarted).ToString(),
                    assessment?.Notes,
                    assessment is null
                        ? null
                        : assessorNames.GetValueOrDefault(assessment.AssessedByUserId),
                    assessment?.AssessedAt);
            })
            .ToList();

        var masteredOrProficient = latestByCompetency.Values
            .Count(assessment => assessment.Level is CompetencyLevel.Proficient or CompetencyLevel.Mastered);
        var populatedStages = stages
            .Select(stage => new
            {
                Stage = stage,
                Competencies = competencies.Where(item => item.StageId == stage.Id).ToList(),
            })
            .Where(item => item.Competencies.Count > 0)
            .ToList();
        var currentStage = populatedStages.FirstOrDefault(item => item.Competencies.Any(competency =>
                !latestByCompetency.TryGetValue(competency.Id, out var assessment)
                || assessment.Level is not (CompetencyLevel.Proficient or CompetencyLevel.Mastered)))
            ?? populatedStages.LastOrDefault();
        var currentStageComplete = currentStage?.Competencies.Count(competency =>
            latestByCompetency.TryGetValue(competency.Id, out var assessment)
            && assessment.Level is CompetencyLevel.Proficient or CompetencyLevel.Mastered) ?? 0;
        var currentStageProgress = currentStage is null
            ? 0
            : currentStageComplete * 100 / currentStage.Competencies.Count;

        return new CompetencySummary(
            youth.Id,
            youth.DisplayName,
            currentStage?.Stage.Name,
            currentStageProgress,
            items,
            masteredOrProficient,
            items.Count);
    }
}
