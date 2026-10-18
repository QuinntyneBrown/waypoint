using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Curriculum;

// Pure behaviour of the ffmpeg command builder and the ffprobe reader; no processes are started.
// Traces to: L2-CURRIC-002 (AC7 adaptive bitrate ladder)
[TestFixture]
[Property("Req", "L2-CURRIC-002")]
public sealed class FfmpegCommandTests
{
    [TestCase(2160, new[] { 1080, 720, 480, 360 })]
    [TestCase(1080, new[] { 1080, 720, 480, 360 })]
    [TestCase(720, new[] { 720, 480, 360 })]
    [TestCase(400, new[] { 360 })]
    [TestCase(240, new[] { 360 })]
    public void The_ladder_never_exceeds_the_source_height(int sourceHeight, int[] expected) =>
        Assert.That(
            FfmpegVideoTranscoder.SelectRenditions(sourceHeight).Select(r => r.Height), Is.EqualTo(expected));

    [Test]
    public void A_phone_video_rotated_by_ninety_degrees_reports_its_displayed_size()
    {
        var probe = FfmpegVideoTranscoder.Parse("""
            {"streams":[{"codec_type":"video","width":1920,"height":1080,
              "side_data_list":[{"rotation":-90}]},{"codec_type":"audio"}],
             "format":{"duration":"125.5"}}
            """);

        Assert.That(probe, Is.EqualTo(new VideoProbe(125.5, 1080, 1920, true)));
    }

    [TestCase("""{"streams":[{"codec_type":"audio"}],"format":{"duration":"10"}}""")]
    [TestCase("""{"streams":[{"codec_type":"video","width":640,"height":360}],"format":{}}""")]
    [TestCase("not json")]
    public void A_file_without_a_readable_video_stream_is_not_a_video(string json) =>
        Assert.That(FfmpegVideoTranscoder.Parse(json), Is.Null);

    [Test]
    public void The_command_decodes_once_and_writes_aligned_six_second_cmaf_renditions()
    {
        var renditions = FfmpegVideoTranscoder.SelectRenditions(720);

        var arguments = FfmpegVideoTranscoder.BuildArguments(
            "in.mp4", @"C:\media\work\abc", new VideoProbe(60, 1280, 720), renditions);
        var line = string.Join(' ', arguments);

        Assert.Multiple(() =>
        {
            Assert.That(arguments.Count(a => a == "-i"), Is.EqualTo(1));
            Assert.That(line, Does.Contain("split=3[v0][v1][v2]"));
            Assert.That(line, Does.Contain("-hls_time 6"));
            Assert.That(line, Does.Contain("-hls_segment_type fmp4"));
            Assert.That(line, Does.Contain("-hls_flags independent_segments"));
            Assert.That(line, Does.Contain("-force_key_frames expr:gte(t,n_forced*2)"));
            Assert.That(line, Does.Contain("v:0,a:0,name:720p v:1,a:1,name:480p v:2,a:2,name:360p"));
            Assert.That(line, Does.Contain("-b:v:0 2800k"));
            Assert.That(line, Does.Contain("C:/media/work/abc/%v/seg_%05d.m4s"));
        });
    }

    [Test]
    public void A_video_without_audio_maps_no_audio_stream()
    {
        var arguments = FfmpegVideoTranscoder.BuildArguments(
            "in.mp4", "/work", new VideoProbe(60, 640, 360, HasAudio: false),
            FfmpegVideoTranscoder.SelectRenditions(360));
        var line = string.Join(' ', arguments);

        Assert.Multiple(() =>
        {
            Assert.That(line, Does.Not.Contain("0:a:0"));
            Assert.That(line, Does.Not.Contain("-c:a"));
            Assert.That(line, Does.Contain("v:0,name:360p"));
        });
    }
}
