namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record AddQuestionRequest(
    string Text,
    QuestionType Type,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> CorrectIndexes);
