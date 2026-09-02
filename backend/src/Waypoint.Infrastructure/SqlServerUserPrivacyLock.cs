using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Infrastructure;

/// <summary>
/// Uses a transaction-owned SQL Server application lock to serialize account delivery state with
/// correction/removal. SQL Server releases the lock only when the ambient transaction completes.
/// </summary>
public sealed class SqlServerUserPrivacyLock : IUserPrivacyLock
{
    private const int MaximumLockTimeoutMilliseconds = 590_000;
    private const int CommandTimeoutMarginSeconds = 10;
    private const int MaximumCommandTimeoutSeconds = 600;
    private readonly AppDbContext _db;
    private readonly int _lockTimeoutMilliseconds;
    private readonly int _commandTimeoutSeconds;

    public SqlServerUserPrivacyLock(
        AppDbContext db,
        IOptions<SmtpOptions> smtpOptions,
        TimeSpan? timeoutOverride = null)
    {
        _db = db;
        var timeout = timeoutOverride
            ?? TimeSpan.FromSeconds(smtpOptions.Value.TimeoutSeconds + 60);
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutOverride), "The privacy-lock timeout must be positive.");
        }

        // SMTP configuration permits at most a 360-second lock wait. Keep direct overrides bounded
        // too, and reserve enough provider-command time for sp_getapplock to return its own timeout
        // result so contention consistently maps to the domain-level 409 below.
        _lockTimeoutMilliseconds = (int)Math.Min(
            Math.Ceiling(timeout.TotalMilliseconds),
            MaximumLockTimeoutMilliseconds);
        _commandTimeoutSeconds = Math.Min(
            (int)Math.Ceiling(_lockTimeoutMilliseconds / 1000d) + CommandTimeoutMarginSeconds,
            MaximumCommandTimeoutSeconds);
    }

    public async Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!_db.Database.IsSqlServer())
        {
            return;
        }

        var transaction = _db.Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "Account-directed delivery and privacy changes require an atomic database transaction.");
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandTimeout = _commandTimeoutSeconds;
        command.CommandText = """
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = @lockTimeout;
            SELECT @result;
            """;

        var resource = command.CreateParameter();
        resource.ParameterName = "@resource";
        resource.DbType = DbType.String;
        resource.Size = 255;
        resource.Value = $"Waypoint:UserPrivacy:{userId:N}";
        command.Parameters.Add(resource);

        var timeout = command.CreateParameter();
        timeout.ParameterName = "@lockTimeout";
        timeout.DbType = DbType.Int32;
        timeout.Value = _lockTimeoutMilliseconds;
        command.Parameters.Add(timeout);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (result < 0)
        {
            throw new ConflictException(
                "Account privacy state is busy. Retry the request in a moment.");
        }
    }
}
