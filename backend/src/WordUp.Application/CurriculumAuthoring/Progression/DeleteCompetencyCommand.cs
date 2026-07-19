using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Progression;

public sealed record DeleteCompetencyCommand(Guid CompetencyId) : IRequest;
