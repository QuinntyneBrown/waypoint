using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Waypoint.IntegrationTests;

public sealed record CapturedLogEntry(
    string Category,
    LogLevel Level,
    string Message,
    Exception? Exception,
    IReadOnlyDictionary<string, object?> Properties);

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<CapturedLogEntry> _entries = new();

    public IReadOnlyList<CapturedLogEntry> Entries => _entries.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

    public void Clear()
    {
        while (_entries.TryDequeue(out _)) { }
    }

    public void Dispose() { }

    private sealed class CapturingLogger(string category, CapturingLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = new Dictionary<string, object?>(StringComparer.Ordinal);
            AddProperties(state, properties);
            provider._entries.Enqueue(new CapturedLogEntry(
                category,
                logLevel,
                formatter(state, exception),
                exception,
                properties));
        }

        private static void AddProperties(object? state, IDictionary<string, object?> target)
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> values)
            {
                foreach (var (key, value) in values) target[key] = value;
            }
        }

        private sealed class NoopScope : IDisposable
        {
            public static NoopScope Instance { get; } = new();
            public void Dispose() { }
        }
    }
}
