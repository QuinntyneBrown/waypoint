using WordUp.Application.Abstractions;

namespace WordUp.Infrastructure;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
