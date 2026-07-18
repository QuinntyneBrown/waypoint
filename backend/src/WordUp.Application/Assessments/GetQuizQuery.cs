using MediatR;

namespace WordUp.Application.Assessments;

public record GetQuizQuery(Guid Id) : IRequest<QuizDetail>;
