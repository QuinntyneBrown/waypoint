namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record CreateQuizRequest(
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule);
