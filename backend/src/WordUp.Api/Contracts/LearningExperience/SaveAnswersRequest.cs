namespace WordUp.Api.Contracts.LearningExperience;

public record SaveAnswersRequest(string AnswersJson, int CurrentQuestionIndex);
