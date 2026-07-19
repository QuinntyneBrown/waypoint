namespace WordUp.Application.PlatformServices.Files;

public static class SafeImageUploadPolicy
{
    public const int MaxBytes = 5 * 1024 * 1024;

    public static bool IsSupportedContentType(string contentType) =>
        contentType is "image/png" or "image/jpeg";

    public static bool ExtensionMatches(string fileName, string contentType)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        return contentType == "image/png" && extension == ".png"
            || contentType == "image/jpeg" && extension is ".jpg" or ".jpeg";
    }

    public static bool HasValidSignature(string contentType, byte[] content)
    {
        if (contentType == "image/png")
        {
            byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
            return content.Length >= signature.Length
                && content.AsSpan(0, signature.Length).SequenceEqual(signature);
        }
        return contentType == "image/jpeg"
            && content.Length >= 3
            && content[0] == 0xff
            && content[1] == 0xd8
            && content[2] == 0xff;
    }
}
