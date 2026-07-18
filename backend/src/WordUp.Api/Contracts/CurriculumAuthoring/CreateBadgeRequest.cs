namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record CreateBadgeRequest(
    string Name,
    string Icon,
    string? Description,
    BadgeCriteriaKind CriteriaKind,
    string? AutoRule,
    Guid? CompetencyId);
