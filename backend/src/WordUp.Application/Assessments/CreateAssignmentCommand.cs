using MediatR;

namespace WordUp.Application.Assessments;

public record CreateAssignmentCommand(
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    IReadOnlyList<RubricInput> Rubric) : IRequest<AssignmentResult>;

public record RubricInput(string Description, int Points);
