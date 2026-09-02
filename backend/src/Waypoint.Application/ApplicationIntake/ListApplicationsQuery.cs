using MediatR;

namespace Waypoint.Application.ApplicationIntake;

/// <summary>The admin review queue: submitted applications, newest first, optionally filtered by status
/// (L2-INTAKE-002 AC1). Admin-only — enforced at the controller (AC5).</summary>
public record ListApplicationsQuery(string? Status = null) : IRequest<IReadOnlyList<ApplicationSummary>>;

public record ApplicationSummary(
    Guid Id,
    string FirstName,
    string LastName,
    int Age,
    bool IsMinor,
    string Neighbourhood,
    string? TrackInterest,
    string Status,
    DateTime CreatedAt);
