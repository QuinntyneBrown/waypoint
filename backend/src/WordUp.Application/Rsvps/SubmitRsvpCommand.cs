using MediatR;
using WordUp.Domain;

namespace WordUp.Application.Rsvps;

/// <summary>A youth RSVPs to a session/event (L2-ENGAGE-005). One RSVP per (session, youth); re-submitting
/// updates the existing row. The optional note is delivered to the mentor.</summary>
public record SubmitRsvpCommand(Guid SessionId, string Response, string? Note, bool ReminderSet)
    : IRequest<RsvpResult>
{
    /// <summary>Parses a free-form response ("going"/"maybe"/"cant"/"cantattend"/"can't make it") to
    /// the canonical <see cref="RsvpResponse"/>. Punctuation/whitespace/case are ignored.</summary>
    public static bool TryParseResponse(string? value, out RsvpResponse response)
    {
        response = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = new string(value.Where(char.IsLetter).ToArray()).ToLowerInvariant();
        switch (normalized)
        {
            case "going":
                response = RsvpResponse.Going;
                return true;
            case "maybe":
                response = RsvpResponse.Maybe;
                return true;
            case "cant":
            case "cantattend":
            case "cantmakeit":
                response = RsvpResponse.CantAttend;
                return true;
            default:
                return false;
        }
    }
}

public record RsvpResult(Guid Id);
