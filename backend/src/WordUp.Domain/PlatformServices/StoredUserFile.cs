namespace WordUp.Domain.PlatformServices;

public enum StoredUserFilePurpose
{
    ProfilePhoto = 1,
    SupportScreenshot = 2,
}

/// <summary>Private user-owned binary content whose purpose determines its access policy.</summary>
public sealed class StoredUserFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public StoredUserFilePurpose Purpose { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
