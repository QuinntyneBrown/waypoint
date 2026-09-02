using MediatR;

namespace Waypoint.Application.GuardianPortal;

/// <summary>A guardian edits their own contact details: display name + email on their User, and
/// relationship / phone / emergency contact on their GuardianProfile (upserted). Only ever their own
/// row (L2-GUARDIAN-003 AC2/AC4, L2-AUTH-010).</summary>
public record UpdateGuardianContactCommand(
    string DisplayName,
    string Email,
    string? Relationship,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone) : IRequest;
