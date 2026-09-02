using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Progression;

public sealed record DeleteCompetencyCommand(Guid CompetencyId) : IRequest;
