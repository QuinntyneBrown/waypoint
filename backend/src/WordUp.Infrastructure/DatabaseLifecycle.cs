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

    public DatabaseLifecycle(AppDbContext db) => _db = db;

    public Task MigrateAsync(CancellationToken cancellationToken = default) =>
        _db.Database.IsRelational()
            ? _db.Database.MigrateAsync(cancellationToken)
            : _db.Database.EnsureCreatedAsync(cancellationToken);

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureDeletedAsync(cancellationToken);
        await MigrateAsync(cancellationToken);
    }
}
