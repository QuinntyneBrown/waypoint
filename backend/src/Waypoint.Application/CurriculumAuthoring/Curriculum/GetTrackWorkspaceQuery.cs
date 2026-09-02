using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public sealed record GetTrackWorkspaceQuery(Guid TrackId) : IRequest<TrackWorkspaceView>;

public sealed record TrackWorkspaceView(
    Guid Id,
    string Name,
    string Key,
    string? Description,
    string Status,
    bool Enabled,
    IReadOnlyList<string> AssignedCohortNames,
    IReadOnlyList<ModuleWorkspaceView> Modules);

public sealed record ModuleWorkspaceView(
    Guid Id,
    string Name,
    string? Description,
    int Order,
    Guid? PrerequisiteModuleId,
    string? ScriptureAnchor,
    string Status,
    IReadOnlyList<LessonWorkspaceView> Lessons,
    IReadOnlyList<QuizWorkspaceView> Quizzes,
    IReadOnlyList<AssignmentWorkspaceView> Assignments);

public sealed record LessonWorkspaceView(
    Guid Id,
    string Title,
    int Order,
    string Status,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes,
    IReadOnlyList<LessonMediaWorkspaceView> Media,
    IReadOnlyList<QuizWorkspaceView> Quizzes,
    IReadOnlyList<AssignmentWorkspaceView> Assignments);

public sealed record LessonMediaWorkspaceView(
    Guid Id,
    string MediaType,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);

public sealed record QuizWorkspaceView(
    Guid Id,
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule,
    int QuestionCount);

public sealed record AssignmentWorkspaceView(
    Guid Id,
    Guid? ModuleId,
    Guid? LessonId,
    string Title,
    string TaskDescription,
    IReadOnlyList<string> SubmissionTypes,
    DateTime? DueDate,
    int GracePeriodDays,
    bool AllowLate,
    bool RequireMentorApproval,
    int TotalPoints,
    string Status);
