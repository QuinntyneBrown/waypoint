namespace WordUp.Application.Shared.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
