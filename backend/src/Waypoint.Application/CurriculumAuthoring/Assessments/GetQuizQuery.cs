using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public record GetQuizQuery(Guid Id) : IRequest<QuizDetail>;
