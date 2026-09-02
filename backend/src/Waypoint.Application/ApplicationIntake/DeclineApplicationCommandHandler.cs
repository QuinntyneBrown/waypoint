using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ApplicationIntake;

public class DeclineApplicationCommandHandler : IRequestHandler<DeclineApplicationCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailSender _email;

    public DeclineApplicationCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IEmailSender email)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Handle(DeclineApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");
        if (application.Status is ApplicationStatus.Approved or ApplicationStatus.Declined)
            throw new ConflictException("This application already has a final decision.");
        if (application.CreatedYouthUserId is not null)
            throw new ConflictException("A linked youth account must be resolved before declining.");

        // Declining creates no account (L2-INTAKE-002 AC4).
        application.Status = ApplicationStatus.Declined;
        application.DecisionNote = request.DecisionNote?.Trim();
        application.ReviewedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "ApplicationDeclined",
            _currentUser.UserId,
            "YouthApplication",
            application.Id,
            cancellationToken: cancellationToken);
        var decisionRecipient = await ApplicationDecisionEmailRecipientResolver.ResolveAsync(
            _db,
            application,
            cancellationToken);
        if (decisionRecipient is not null)
        {
            await _email.SendAsync(
                decisionRecipient.Email,
                "An update on your Waypoint application",
                $"We are unable to offer {application.FirstName} {application.LastName} a place " +
                "at this time. Waypoint staff can answer any questions about this decision.",
                cancellationToken,
                subjectUserId: application.ApplicantUserId,
                recipientUserId: decisionRecipient.UserId);
        }
    }
}
