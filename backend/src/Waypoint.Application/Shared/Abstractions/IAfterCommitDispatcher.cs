namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// Runs non-durable side effects best-effort, deferring them when the infrastructure has an active
/// managed transaction so they cannot become visible before a successful commit.
/// </summary>
public interface IAfterCommitDispatcher
{
    Task DispatchBestEffortAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
