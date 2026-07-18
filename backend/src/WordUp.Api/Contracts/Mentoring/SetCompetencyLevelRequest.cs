namespace WordUp.Api.Contracts.Mentoring;

public record SetCompetencyLevelRequest(Guid CompetencyId, string Level, string? Notes);
