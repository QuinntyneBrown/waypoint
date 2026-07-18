using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Assessments;

public record CreateQuizCommand(
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule) : IRequest<QuizResult>;
