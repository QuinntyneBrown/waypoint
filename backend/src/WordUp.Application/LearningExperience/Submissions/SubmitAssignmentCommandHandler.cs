using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using WordUp.Application.ConsentAndSafeguarding;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.Submissions;

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, SubmissionResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationDeliveryService _notifications;

    public SubmitAssignmentCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<SubmissionResult> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId
            ?? throw new ForbiddenException("No current user.");

        var assignment = await AssignmentAccessGuard.EnsureAccessibleAsync(
            _db, request.AssignmentId, youthId, cancellationToken);
        if (await _db.Submissions.AnyAsync(
                item => item.YouthUserId == youthId && item.AssignmentId == assignment.Id,
                cancellationToken))
        {
            throw new ConflictException("This assignment has already been submitted.");
        }

        var allowed = AssignmentAccessGuard.SubmissionTypes(assignment);
        var hasText = !string.IsNullOrWhiteSpace(request.BodyText);
        var hasPhoto = request.PhotoUploadId is not null;
        var hasFile = request.FileUploadId is not null;
        var hasLink = !string.IsNullOrWhiteSpace(request.LinkUrl);
        var errors = new List<ValidationFailure>();
        if (hasText && !allowed.Contains("text")) errors.Add(new("bodyText", "Written responses are not allowed."));
        if (hasPhoto && !allowed.Contains("photo")) errors.Add(new("photoUploadId", "Photos are not allowed."));
        if (hasFile && !allowed.Contains("file")) errors.Add(new("fileUploadId", "Files are not allowed."));
        if (hasLink && !allowed.Contains("link")) errors.Add(new("linkUrl", "Links are not allowed."));
        if (!hasText && !hasPhoto && !hasFile && !hasLink)
            errors.Add(new("submission", "Provide one of the allowed submission types."));
        if (allowed.Count == 1 && allowed[0] == "text" && !hasText)
            errors.Add(new("bodyText", "A written response is required."));
        if (allowed.Count == 1 && allowed[0] == "photo" && !hasPhoto)
            errors.Add(new("photoUploadId", "A photo is required."));
        if (allowed.Count == 1 && allowed[0] == "file" && !hasFile)
            errors.Add(new("fileUploadId", "A file is required."));
        if (allowed.Count == 1 && allowed[0] == "link" && !hasLink)
            errors.Add(new("linkUrl", "A link is required."));
        if (hasLink && (!Uri.TryCreate(request.LinkUrl, UriKind.Absolute, out var link)
            || link.Scheme is not ("http" or "https")))
            errors.Add(new("linkUrl", "Enter a valid HTTP or HTTPS link."));
        if (errors.Count > 0) throw new ValidationException(errors);

        if (hasPhoto && !await ConsentPolicyEvaluator.IsYouthMediaUseAllowedAsync(
                _db,
                youthId,
                YouthMediaUse.IndividualPhotoOrVideo,
                _clock.UtcNow,
                cancellationToken))
        {
            throw new MediaConsentRequiredException();
        }

        StoredSubmissionFile? photo = null;
        if (request.PhotoUploadId is { } photoId)
        {
            photo = await _db.StoredSubmissionFiles.FirstOrDefaultAsync(
                item => item.Id == photoId,
                cancellationToken) ?? throw new NotFoundException("Uploaded photo not found.");
            if (photo.OwnerUserId != youthId || photo.AssignmentId != assignment.Id)
                throw new ForbiddenException("This upload cannot be used for this assignment.");
            if (photo.Kind != SubmissionFileKind.Photo)
                throw new ValidationException([new("photoUploadId", "Choose a validated photo upload.")]);
        }

        StoredSubmissionFile? file = null;
        if (request.FileUploadId is { } fileId)
        {
            file = await _db.StoredSubmissionFiles.FirstOrDefaultAsync(
                item => item.Id == fileId,
                cancellationToken) ?? throw new NotFoundException("Uploaded file not found.");
            if (file.OwnerUserId != youthId || file.AssignmentId != assignment.Id)
                throw new ForbiddenException("This upload cannot be used for this assignment.");
            if (file.Kind != SubmissionFileKind.File)
                throw new ValidationException([new("fileUploadId", "Choose a validated PDF upload.")]);
        }

        var now = _clock.UtcNow;
        var submission = new Submission
        {
            YouthUserId = youthId,
            AssignmentId = request.AssignmentId,
            BodyText = request.BodyText,
            PhotoUploadId = photo?.Id,
            PhotoUrl = photo is null ? null : $"/api/submission-files/{photo.Id}",
            FileUploadId = file?.Id,
            LinkUrl = request.LinkUrl,
            IsCodingProject = request.IsCodingProject,
            YouthNote = request.YouthNote,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = now,
            CreatedAt = now,
        };

        _db.Submissions.Add(submission);
        _db.SubmissionTimelineEvents.Add(new SubmissionTimelineEvent
        {
            SubmissionId = submission.Id,
            Type = "Submitted",
            OccurredAt = now,
        });
        var selectedIds = new[] { photo?.Id, file?.Id }
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToList();
        var unusedUploads = await _db.StoredSubmissionFiles
            .Where(item => item.OwnerUserId == youthId
                && item.AssignmentId == assignment.Id
                && !selectedIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        _db.StoredSubmissionFiles.RemoveRange(unusedUploads);

        NotificationDeliveryPlan? approvalPlan = null;
        var submitter = await _db.Users.AsNoTracking()
            .Where(item => item.Id == youthId)
            .Select(item => new { item.DisplayName, item.MentorId })
            .SingleAsync(cancellationToken);
        if (submitter.MentorId is { } mentorId)
        {
            approvalPlan = await _notifications.PrepareAsync(
                mentorId,
                "approval-needed",
                "Assignment ready for review",
                "A participant submitted an assignment.",
                $"/mentor/reviews/submissions/{submission.Id}",
                now,
                cancellationToken,
                subjectUserId: youthId);
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (approvalPlan is not null)
        {
            await _notifications.DispatchAsync(approvalPlan, cancellationToken);
        }

        await _audit.LogAsync("SubmissionCreated", youthId, "Submission", submission.Id);

        return new SubmissionResult(submission.Id);
    }
}
