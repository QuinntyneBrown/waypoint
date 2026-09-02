namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// Marks an application request whose separate SaveChanges calls must commit as one relational
/// database transaction.
/// </summary>
public interface IAtomicRequest;
