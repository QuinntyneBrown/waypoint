using Microsoft.EntityFrameworkCore;

namespace WordUp.Infrastructure;

public interface IDatabaseLifecycle
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}

public sealed class DatabaseLifecycle : IDatabaseLifecycle
{
    private readonly AppDbContext _db;
    private readonly SensitiveDataBackfill _sensitiveDataBackfill;

    public DatabaseLifecycle(AppDbContext db, SensitiveDataBackfill sensitiveDataBackfill)
    {
        _db = db;
        _sensitiveDataBackfill = sensitiveDataBackfill;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsRelational())
        {
            await _db.Database.MigrateAsync(cancellationToken);
            await _sensitiveDataBackfill.RunAsync(cancellationToken);
            return;
        }

        await _db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureDeletedAsync(cancellationToken);
        await MigrateAsync(cancellationToken);
    }
}
