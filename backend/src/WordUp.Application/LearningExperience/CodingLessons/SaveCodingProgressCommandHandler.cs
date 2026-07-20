using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.CodingLessons;

public sealed class SaveCodingProgressCommandHandler : IRequestHandler<SaveCodingProgressCommand, CodingProgressResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IChangePublisher _publisher;
    private readonly IAfterCommitDispatcher _afterCommit;

    public SaveCodingProgressCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IChangePublisher publisher,
        IAfterCommitDispatcher afterCommit)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _publisher = publisher;
        _afterCommit = afterCommit;
    }

    public async Task<CodingProgressResult> Handle(SaveCodingProgressCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var access = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
            _db, request.LessonId, youthId, cancellationToken);
        if (!access.Lesson.IsCodingLesson) throw new NotFoundException("Coding lesson not found.");

        var stepIds = await _db.CodingLessonSteps
            .Where(item => item.LessonId == access.Lesson.Id)
            .OrderBy(item => item.Order)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var checkedIds = request.CheckedStepIds.Distinct().ToList();
        if (checkedIds.Any(id => !stepIds.Contains(id)))
        {
            throw new ValidationException([
                new ValidationFailure("checkedStepIds", "The checklist contains a step outside this coding lesson."),
            ]);
        }

        var now = _clock.UtcNow;
        var completed = stepIds.Count > 0 && checkedIds.Count == stepIds.Count;
        var workspace = await _db.CodingLessonWorkspaces.FirstOrDefaultAsync(
            item => item.YouthUserId == youthId && item.LessonId == access.Lesson.Id,
            cancellationToken);
        if (workspace is null)
        {
            workspace = new CodingLessonWorkspace { YouthUserId = youthId, LessonId = access.Lesson.Id };
            _db.CodingLessonWorkspaces.Add(workspace);
        }
        workspace.Code = request.Code;
        workspace.CheckedStepIdsJson = JsonSerializer.Serialize(checkedIds);
        workspace.UpdatedAt = now;
        if (completed) workspace.CompletedAt ??= now;

        var progress = await _db.LessonProgress.FirstOrDefaultAsync(
            item => item.YouthUserId == youthId && item.LessonId == access.Lesson.Id,
            cancellationToken);
        if (progress is null)
        {
            progress = new LessonProgress { YouthUserId = youthId, LessonId = access.Lesson.Id };
            _db.LessonProgress.Add(progress);
        }
        progress.Status = completed || progress.Status == ProgressStatus.Completed
            ? ProgressStatus.Completed
            : ProgressStatus.InProgress;
        progress.UpdatedAt = now;
        if (completed) progress.CompletedAt ??= now;

        var badgeEarned = await AwardBadgeAsync(access.Lesson.Id, youthId, completed, now, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            completed ? "CodingLessonCompleted" : "CodingLessonProgressSaved",
            youthId,
            "Lesson",
            access.Lesson.Id,
            cancellationToken: cancellationToken);
        var cohortId = await _db.Users
            .Where(user => user.Id == youthId)
            .Select(user => user.CohortId)
            .FirstOrDefaultAsync(cancellationToken);
        if (cohortId is { } progressCohortId)
        {
            await _afterCommit.DispatchBestEffortAsync(
                callbackCancellationToken => _publisher.PublishToRoleAsync(
                    Roles.ProgramAdministrator,
                    "progressChanged",
                    new { cohortId = progressCohortId, youthId },
                    callbackCancellationToken),
                cancellationToken);
        }

        var percent = stepIds.Count == 0 ? 0 : checkedIds.Count * 100 / stepIds.Count;
        return new CodingProgressResult(checkedIds.Count, stepIds.Count, percent, workspace.CompletedAt is not null, badgeEarned);
    }

    private async Task<bool> AwardBadgeAsync(
        Guid lessonId,
        Guid youthId,
        bool completed,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (!completed) return false;
        var badgeIds = await _db.Badges
            .Where(item => item.ArchivedAt == null
                && item.AutoRule == "coding-checklist-completed"
                && (item.TargetRefId == lessonId || item.TargetRefId == null))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var earned = await _db.BadgeAwards
            .Where(item => item.YouthUserId == youthId && badgeIds.Contains(item.BadgeId))
            .Select(item => item.BadgeId)
            .ToListAsync(cancellationToken);
        foreach (var badgeId in badgeIds.Except(earned))
        {
            _db.BadgeAwards.Add(new BadgeAward
            {
                YouthUserId = youthId,
                BadgeId = badgeId,
                AwardedAt = now,
            });
        }
        return badgeIds.Count > 0;
    }
}
