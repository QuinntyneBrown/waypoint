
namespace Waypoint.Application.CurriculumAuthoring.Assessments;

public record QuizDetail(
    Guid Id,
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule,
    int QuestionCount,
    IReadOnlyList<QuizQuestionDetail> Questions);

public record QuizQuestionDetail(
    Guid Id,
    string Text,
    QuestionType Type,
    int Order,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> CorrectIndexes,
    IReadOnlyList<string> CorrectAnswers);
