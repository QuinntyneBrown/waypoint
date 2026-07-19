namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public static class LessonMediaUploadPolicy
{
    public const int MaxBytes = 25 * 1024 * 1024;

    public static bool HasValidSignature(string contentType, byte[] content)
    {
        if (contentType == "application/pdf")
        {
            return content.Length >= 5 && content.AsSpan(0, 5).SequenceEqual("%PDF-"u8);
        }
        if (contentType == "video/mp4")
        {
            return content.Length >= 12 && content.AsSpan(4, 4).SequenceEqual("ftyp"u8);
        }
        return false;
    }

    public static bool ExtensionMatches(string fileName, string contentType)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        return contentType == "application/pdf" && extension == ".pdf"
            || contentType == "video/mp4" && extension == ".mp4";
    }

    public static string MediaType(string contentType) =>
        contentType == "video/mp4" ? "video" : "document";
}
