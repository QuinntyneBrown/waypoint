using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

internal static class AssignmentAccessGuard
{
    public static async Task<Assignment> EnsureAccessibleAsync(
        IAppDbContext db,
        Guid assignmentId,
        Guid youthId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.Assignments
            .FirstOrDefaultAsync(item => item.Id == assignmentId, cancellationToken)
            ?? throw new NotFoundException("Assignment not found.");
        if (assignment.LessonId is { } lessonId)
        {
            await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
                db, lessonId, youthId, cancellationToken);
        }
        else if (assignment.ModuleId is { } moduleId)
        {
            var module = await db.Modules.FirstOrDefaultAsync(item => item.Id == moduleId, cancellationToken)
                ?? throw new NotFoundException("Module not found.");
            var trackIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
                db, youthId, cancellationToken);
            if (module.Status != PublishStatus.Published || !trackIds.Contains(module.TrackId))
            {
                throw new ForbiddenException("This assignment is not available to you.");
            }
        }

        return assignment;
    }

    public static IReadOnlyList<string> SubmissionTypes(Assignment assignment) =>
        assignment.SubmissionTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.ToLowerInvariant())
            .Distinct()
            .ToList();
}
