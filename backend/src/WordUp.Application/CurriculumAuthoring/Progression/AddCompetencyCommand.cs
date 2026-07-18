using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public record AddCompetencyCommand(Guid StageId, string Name) : IRequest<CompetencyResult>;
