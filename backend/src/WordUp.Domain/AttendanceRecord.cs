namespace WordUp.Domain;

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

    // Concurrency token so a second save on the same session cannot silently overwrite (L2-ENGAGE-002 AC5).
    public byte[]? RowVersion { get; set; }
}
