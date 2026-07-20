using WordUp.Application.Shared.Abstractions;

namespace WordUp.Cli;

public sealed class OperationalChangePublisher : IChangePublisher
{
    public Task PublishToUserAsync(
        Guid userId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PublishToRoleAsync(
        string role,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PublishToCohortAsync(
        Guid cohortId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
