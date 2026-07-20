using Microsoft.Extensions.Logging;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

/// <summary>Scoped callback buffer coordinated by managed application transactions.</summary>
public sealed class AfterCommitDispatcher : IAfterCommitDispatcher
{
    private static readonly TimeSpan CallbackTimeout = TimeSpan.FromSeconds(5);
    private readonly object _gate = new();
    private readonly List<Func<CancellationToken, Task>> _pending = [];
    private readonly ILogger<AfterCommitDispatcher> _logger;
    private int _deferralDepth;

    public AfterCommitDispatcher(ILogger<AfterCommitDispatcher> logger) => _logger = logger;

    public bool IsDeferring
    {
        get
        {
            lock (_gate) return _deferralDepth > 0;
        }
    }

    public IDisposable Defer()
    {
        lock (_gate) _deferralDepth++;
        return new Deferral(this);
    }

    public Task DispatchBestEffortAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (_gate)
        {
            if (_deferralDepth > 0)
            {
                _pending.Add(action);
                return Task.CompletedTask;
            }
        }
        return RunBestEffortAsync(action, cancellationToken);
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        List<Func<CancellationToken, Task>> actions;
        lock (_gate)
        {
            actions = [.. _pending];
            _pending.Clear();
        }
        await Task.WhenAll(actions.Select(action =>
            RunBestEffortAsync(action, cancellationToken)));
    }

    public void Discard()
    {
        lock (_gate) _pending.Clear();
    }

    private async Task RunBestEffortAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CallbackTimeout);
        try
        {
            await action(timeout.Token);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                "Post-commit callback failed with {ExceptionType}.",
                exception.GetType().Name);
        }
    }

    private void EndDeferral()
    {
        lock (_gate)
        {
            if (_deferralDepth <= 0)
                throw new InvalidOperationException("No post-commit deferral is active.");
            _deferralDepth--;
        }
    }

    private sealed class Deferral(AfterCommitDispatcher owner) : IDisposable
    {
        private AfterCommitDispatcher? _owner = owner;

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.EndDeferral();
    }
}
