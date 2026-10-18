namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public static class LessonVideoUploadPolicy
{
    public static bool HasVideoExtension(string fileName) =>
        Path.GetExtension(Path.GetFileName(fileName)).Equals(".mp4", StringComparison.OrdinalIgnoreCase);

    /// <summary>An MP4 file carries the "ftyp" box marker at offset 4.</summary>
    public static bool HasMp4Signature(ReadOnlySpan<byte> head) =>
        head.Length >= 12 && head.Slice(4, 4).SequenceEqual("ftyp"u8);
}
