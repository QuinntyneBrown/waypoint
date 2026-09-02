namespace Waypoint.Domain.Engagement;

/// <summary>
/// Durable receipt for an attendance write carrying an idempotency key. The unique owner/key pair
/// makes an offline replay return the original result without executing attendance or audit side
/// effects twice (L2-NFR-004).
/// </summary>
public sealed class AttendanceWriteReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestFingerprint { get; set; } = string.Empty;
    public int Present { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public int Absent { get; set; }
    public int Marked { get; set; }
    public int Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
