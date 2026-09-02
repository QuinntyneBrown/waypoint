using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.PlatformServices.Support;

public class SubmitSupportRequestCommandHandler : IRequestHandler<SubmitSupportRequestCommand, SupportRequestResult>
{
    private const string FallbackDirectorInbox = "support@waypoint.local";

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IEmailSender _email;
    private readonly IAuditLogger _audit;

    public SubmitSupportRequestCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IEmailSender email,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _email = email;
        _audit = audit;
    }

    public async Task<SupportRequestResult> Handle(
        SubmitSupportRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        StoredUserFile? screenshot = null;
        if (request.ScreenshotFileId is { } screenshotId)
        {
            screenshot = await _db.StoredUserFiles.FirstOrDefaultAsync(
                item => item.Id == screenshotId
                    && item.Purpose == StoredUserFilePurpose.SupportScreenshot,
                cancellationToken) ?? throw new NotFoundException("Support screenshot not found.");
            if (screenshot.OwnerUserId != userId)
            {
                throw new ForbiddenException("This screenshot does not belong to you.");
            }
        }

        var support = new SupportRequest
        {
            UserId = userId,
            Category = request.Category,
            Description = request.Description,
            ReplyToEmail = request.ReplyToEmail,
            ScreenshotFileId = screenshot?.Id,
            CreatedAt = _clock.UtcNow,
        };

        _db.SupportRequests.Add(support);
        var linkedScreenshotIds = await _db.SupportRequests.AsNoTracking()
            .Where(item => item.ScreenshotFileId != null)
            .Select(item => item.ScreenshotFileId!.Value)
            .ToListAsync(cancellationToken);
        var unusedScreenshots = await _db.StoredUserFiles
            .Where(item => item.OwnerUserId == userId
                && item.Purpose == StoredUserFilePurpose.SupportScreenshot
                && item.Id != request.ScreenshotFileId
                && !linkedScreenshotIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        _db.StoredUserFiles.RemoveRange(unusedScreenshots);
        await _db.SaveChangesAsync(cancellationToken);

        // Route to the configured support inbox (the director); fall back to a platform default.
        var org = await _db.OrgSettings.FirstOrDefaultAsync(cancellationToken);
        var director = string.IsNullOrWhiteSpace(org?.ContactEmail) ? FallbackDirectorInbox : org!.ContactEmail!;

        var subject = $"[Support] {request.Category}";
        var body =
            $"A new support request was submitted.\n\n" +
            $"Category: {request.Category}\n" +
            $"Reply-To: {request.ReplyToEmail}\n" +
            (screenshot is null ? "" : $"Screenshot: /api/support/screenshots/{screenshot.Id}\n") +
            "\n" +
            request.Description;
        await _email.SendAsync(
            director,
            subject,
            body,
            cancellationToken,
            subjectUserId: userId);

        await _audit.LogAsync("SupportRequestSubmitted", _currentUser.UserId, "SupportRequest", support.Id, cancellationToken: cancellationToken);

        return new SupportRequestResult(support.Id);
    }
}
