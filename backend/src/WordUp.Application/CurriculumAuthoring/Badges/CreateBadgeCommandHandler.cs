using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.CurriculumAuthoring.Badges;

public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, BadgeResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateBadgeCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<BadgeResult> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
    {
        var autoRule = Clean(request.AutoRule)?.ToLowerInvariant();
        if (request.TargetRefId is { } targetId)
        {
            var targetExists = autoRule switch
            {
                "module-completed" => await _db.Modules.AnyAsync(item => item.Id == targetId, cancellationToken),
                "lesson-completed" or "coding-checklist-completed" =>
                    await _db.Lessons.AnyAsync(item => item.Id == targetId && item.ArchivedAt == null, cancellationToken),
                "quiz-passed" => await _db.Quizzes.AnyAsync(item => item.Id == targetId, cancellationToken),
                "assignment-approved" => await _db.Assignments.AnyAsync(item => item.Id == targetId, cancellationToken),
                _ => false,
            };

            if (!targetExists)
            {
                throw new NotFoundException("Badge rule target not found.");
            }
        }

        if (request.CompetencyId is { } competencyId
            && !await _db.Competencies.AnyAsync(
                item => item.Id == competencyId && item.ArchivedAt == null,
                cancellationToken))
        {
            throw new NotFoundException("Competency not found.");
        }

        var badge = new Badge
        {
            Name = request.Name.Trim(),
            Icon = request.Icon.Trim(),
            Description = Clean(request.Description),
            CriteriaKind = request.CriteriaKind,
            AutoRule = autoRule,
            TargetRefId = request.TargetRefId,
            CompetencyId = request.CompetencyId,
            CreatedAt = _clock.UtcNow,
        };

        _db.Badges.Add(badge);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            "BadgeCreated",
            _currentUser.UserId,
            "Badge",
            badge.Id,
            cancellationToken: cancellationToken);

        return new BadgeResult(badge.Id);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
