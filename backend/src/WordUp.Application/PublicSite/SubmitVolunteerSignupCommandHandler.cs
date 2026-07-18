using System.Net;
using MediatR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.PublicSite;

public class SubmitVolunteerSignupCommandHandler : IRequestHandler<SubmitVolunteerSignupCommand, SubmitVolunteerSignupResult>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;

    public SubmitVolunteerSignupCommandHandler(IAppDbContext db, IClock clock, IAuditLogger audit)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
    }

    public async Task<SubmitVolunteerSignupResult> Handle(SubmitVolunteerSignupCommand request, CancellationToken cancellationToken)
    {
        // Free-text is HTML-encoded before persistence to neutralise stored XSS (L2-NFR-001, L2-PUB-003 AC4).
        var signup = new VolunteerSignup
        {
            Name = WebUtility.HtmlEncode(request.Name.Trim()),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : WebUtility.HtmlEncode(request.Phone.Trim()),
            AreasOfInterest = string.IsNullOrWhiteSpace(request.AreasOfInterest)
                ? null
                : WebUtility.HtmlEncode(request.AreasOfInterest.Trim()),
            ScreeningConsentAck = request.ScreeningConsentAck,
            CreatedAt = _clock.UtcNow,
        };

        _db.VolunteerSignups.Add(signup);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("VolunteerSignupSubmitted", null, "VolunteerSignup", signup.Id);

        return new SubmitVolunteerSignupResult(
            signup.Id,
            "Thanks for your interest in volunteering — we'll follow up about next steps, including screening.");
    }
}
