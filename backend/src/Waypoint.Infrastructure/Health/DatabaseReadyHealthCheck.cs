using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Waypoint.Infrastructure.Health;

/// <summary>
/// Readiness check that verifies the application's data dependency is reachable (L2-NFR-011 AC1).
/// Tagged "ready" and mapped at <c>/health/ready</c>; the liveness probe at <c>/health</c> runs no
/// checks. For the in-memory provider (tests / local default) CanConnect reports healthy, which is
/// the intended behaviour.
/// </summary>
public sealed class DatabaseReadyHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public DatabaseReadyHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database reachable.")
                : HealthCheckResult.Unhealthy("Database unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database probe failed.", ex);
        }
    }
}
