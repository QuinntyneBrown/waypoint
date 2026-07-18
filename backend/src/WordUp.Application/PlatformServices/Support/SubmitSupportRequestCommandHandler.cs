using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.PlatformServices.Support;

public class SubmitSupportRequestCommandHandler : IRequestHandler<SubmitSupportRequestCommand, SupportRequestResult>
{
    private const string FallbackDirectorInbox = "support@wordup.local";

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
        var support = new SupportRequest
        {
            UserId = _currentUser.UserId,
            Category = request.Category,
            Description = request.Description,
            ReplyToEmail = request.ReplyToEmail,
            ScreenshotUrl = request.ScreenshotUrl,
            CreatedAt = _clock.UtcNow,
        };

        _db.SupportRequests.Add(support);
        await _db.SaveChangesAsync(cancellationToken);

        // Route to the configured support inbox (the director); fall back to a platform default.
        var org = await _db.OrgSettings.FirstOrDefaultAsync(cancellationToken);
        var director = string.IsNullOrWhiteSpace(org?.ContactEmail) ? FallbackDirectorInbox : org!.ContactEmail!;

        var subject = $"[Support] {request.Category}";
        var body =
            $"A new support request was submitted.\n\n" +
            $"Category: {request.Category}\n" +
            $"Reply-To: {request.ReplyToEmail}\n\n" +
            request.Description;
        await _email.SendAsync(director, subject, body, cancellationToken);

        await _audit.LogAsync("SupportRequestSubmitted", _currentUser.UserId, "SupportRequest", support.Id, cancellationToken: cancellationToken);

        return new SupportRequestResult(support.Id);
    }
}
