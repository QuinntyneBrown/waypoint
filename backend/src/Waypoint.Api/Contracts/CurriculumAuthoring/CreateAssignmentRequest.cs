namespace Waypoint.Api.Contracts.CurriculumAuthoring;

public record CreateAssignmentRequest(
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    IReadOnlyList<RubricRequest> Rubric);
