namespace WordUp.Api.Contracts.LearningExperience;

public sealed record SubmitCodingProjectRequest(string Code, string? MentorNote, bool Attest);
