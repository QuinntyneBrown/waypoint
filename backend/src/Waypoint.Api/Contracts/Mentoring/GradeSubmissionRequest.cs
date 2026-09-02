namespace Waypoint.Api.Contracts.Mentoring;

public record GradeSubmissionRequest(
    string Decision,
    string? RubricScoresJson,
    int? TotalAwarded,
    string? Comment,
    bool GuardianNotified,
    string? ExpectedVersion);
