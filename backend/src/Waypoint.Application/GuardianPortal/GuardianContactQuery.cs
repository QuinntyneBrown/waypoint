using MediatR;

namespace Waypoint.Application.GuardianPortal;

public sealed record GuardianContactQuery : IRequest<GuardianContactView>;

public sealed record GuardianContactView(
    string DisplayName,
    string Email,
    string? Relationship,
    string? Phone,
    string? EmergencyContactName,
    string? EmergencyContactPhone);
