using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public record GetQuizQuery(Guid Id) : IRequest<QuizDetail>;
