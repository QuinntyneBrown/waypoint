namespace Waypoint.Api.Contracts.GuardianPortal;

public record UpdateGuardianContactRequest(
    string DisplayName,
    string Email,
    string? Relationship,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone);
