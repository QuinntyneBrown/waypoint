namespace WordUp.Domain.Engagement;

/// <summary>A per-youth attendance mark for a session (L2-ENGAGE-002). One record per (session, youth):
/// saving again upserts rather than duplicating, which is what makes offline sync idempotent
/// (L2-ENGAGE-003 / L2-NFR-004). <see cref="RowVersion"/> is the optimistic-concurrency token
/// (L2-NFR-006).</summary>
public class AttendanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid YouthUserId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }
    public Guid RecordedByUserId { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Database-level optimistic-concurrency token (populated by SQL Server's rowversion).
    public byte[]? RowVersion { get; set; }

    // Application-managed concurrency stamp, advanced on every save. This is the version surfaced to and
    // echoed back by clients so a stale save is rejected (L2-NFR-006) — it works on every EF provider,
    // not just those that generate a rowversion.
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
}
