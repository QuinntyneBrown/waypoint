using MediatR;

namespace WordUp.Application.Progression;

public record AddCompetencyCommand(Guid StageId, string Name) : IRequest<CompetencyResult>;
