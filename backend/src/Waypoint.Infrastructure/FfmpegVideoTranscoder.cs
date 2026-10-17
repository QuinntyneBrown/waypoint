using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>One rung of the adaptive ladder.</summary>
public sealed record Rendition(int Height, int VideoKbps, int MaxRateKbps, int BufferKbit)
{
    public string Name => $"{Height}p";
}

/// <summary>Drives the self-hosted ffprobe and ffmpeg binaries as child processes.</summary>
public sealed class FfmpegVideoTranscoder : IVideoTranscoder
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(30);

    public static readonly IReadOnlyList<Rendition> Ladder =
    [
        new(1080, 5000, 5350, 7500),
        new(720, 2800, 2996, 4200),
        new(480, 1400, 1498, 2100),
        new(360, 800, 856, 1200),
    ];

    private readonly MediaOptions _options;

    public FfmpegVideoTranscoder(IOptions<MediaOptions> options)
    {
        _options = options.Value;
    }

    public async Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken)
    {
        var info = NewProcess(_options.FfprobePath,
            ["-v", "error", "-print_format", "json", "-show_format", "-show_streams", sourcePath]);
        using var process = Process.Start(info)
            ?? throw new InvalidOperationException("ffprobe could not be started.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ProbeTimeout);
        var errors = process.StandardError.ReadToEndAsync(timeout.Token);
        string output;
        try
        {
            output = await process.StandardOutput.ReadToEndAsync(timeout.Token);
            await process.WaitForExitAsync(timeout.Token);
            await errors;
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            if (cancellationToken.IsCancellationRequested) throw;
            return null;
        }

        return process.ExitCode == 0 ? Parse(output) : null;
    }

    public async Task TranscodeAsync(
        string sourcePath,
        string workDirectory,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        var probe = await ProbeAsync(sourcePath, cancellationToken)
            ?? throw new InvalidOperationException("The source is not a decodable video.");
        var renditions = SelectRenditions(probe.Height);
        foreach (var rendition in renditions) Directory.CreateDirectory(Path.Combine(workDirectory, rendition.Name));

        await RunAsync(
            _options.FfmpegPath,
            BuildArguments(sourcePath, workDirectory, probe, renditions),
            line =>
            {
                const string key = "out_time_us=";
                if (line.StartsWith(key, StringComparison.Ordinal)
                    && long.TryParse(line.AsSpan(key.Length), out var micros) && micros >= 0)
                {
                    progress.Report(micros / (probe.DurationSeconds * 1_000_000d));
                }
            },
            cancellationToken);

        var posterAt = Math.Min(5, probe.DurationSeconds / 2).ToString("0.###", CultureInfo.InvariantCulture);
        await RunAsync(
            _options.FfmpegPath,
            ["-hide_banner", "-nostdin", "-y", "-loglevel", "error", "-ss", posterAt, "-i", sourcePath,
             "-frames:v", "1", "-vf", "scale=-2:720", Path.Combine(workDirectory, "poster.jpg")],
            _ => { },
            cancellationToken);
    }

    /// <summary>Renditions no taller than the source; the smallest rung when the source is smaller still.</summary>
    public static IReadOnlyList<Rendition> SelectRenditions(int sourceHeight)
    {
        var selected = Ladder.Where(rung => rung.Height <= sourceHeight).ToList();
        return selected.Count > 0 ? selected : [Ladder[^1]];
    }

    /// <summary>
    /// One decode, split into every rendition: 30 fps yuv420p H.264, key frame every 2 s aligned across
    /// renditions, six second CMAF segments, one folder per rendition and a master playlist.
    /// </summary>
    public static IReadOnlyList<string> BuildArguments(
        string sourcePath, string workDirectory, VideoProbe probe, IReadOnlyList<Rendition> renditions)
    {
        var labels = string.Concat(renditions.Select((_, index) => $"[v{index}]"));
        var scales = string.Join(
            ";", renditions.Select((rung, index) => $"[v{index}]scale=-2:{rung.Height}[o{index}]"));
        var filter = $"[0:v]fps=30,format=yuv420p,split={renditions.Count}{labels};{scales}";

        var arguments = new List<string>
        {
            "-hide_banner", "-nostdin", "-y", "-loglevel", "error", "-progress", "pipe:1", "-nostats",
            "-i", sourcePath, "-filter_complex", filter,
        };
        for (var index = 0; index < renditions.Count; index++)
        {
            arguments.AddRange(["-map", $"[o{index}]"]);
            if (probe.HasAudio) arguments.AddRange(["-map", "0:a:0"]);
        }
        arguments.AddRange(
        [
            "-c:v", "libx264", "-preset", "veryfast", "-profile:v", "high", "-level:v", "4.1",
            "-sc_threshold", "0", "-g", "60", "-keyint_min", "60",
            "-force_key_frames", "expr:gte(t,n_forced*2)",
        ]);
        for (var index = 0; index < renditions.Count; index++)
        {
            var rung = renditions[index];
            arguments.AddRange(
            [
                $"-b:v:{index}", $"{rung.VideoKbps}k",
                $"-maxrate:v:{index}", $"{rung.MaxRateKbps}k",
                $"-bufsize:v:{index}", $"{rung.BufferKbit}k",
            ]);
        }
        if (probe.HasAudio)
        {
            arguments.AddRange(["-c:a", "aac", "-b:a", "128k", "-ac", "2", "-ar", "48000"]);
        }
        var variants = string.Join(" ", renditions.Select((rung, index) =>
            probe.HasAudio
                ? $"v:{index},a:{index},name:{rung.Name}"
                : $"v:{index},name:{rung.Name}"));
        var work = workDirectory.Replace('\\', '/');
        arguments.AddRange(
        [
            "-f", "hls", "-hls_time", "6", "-hls_playlist_type", "vod", "-hls_segment_type", "fmp4",
            "-hls_flags", "independent_segments", "-hls_fmp4_init_filename", "init.mp4",
            "-var_stream_map", variants, "-master_pl_name", "master.m3u8",
            "-hls_segment_filename", $"{work}/%v/seg_%05d.m4s", $"{work}/%v/index.m3u8",
        ]);
        return arguments;
    }

    /// <summary>Reads duration and display size (after rotation) from ffprobe JSON.</summary>
    public static VideoProbe? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("streams", out var streams)) return null;
            var hasAudio = streams.EnumerateArray()
                .Any(stream => stream.GetProperty("codec_type").GetString() == "audio");
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.GetProperty("codec_type").GetString() != "video") continue;
                var width = stream.GetProperty("width").GetInt32();
                var height = stream.GetProperty("height").GetInt32();
                if (Math.Abs(Rotation(stream)) % 180 == 90) (width, height) = (height, width);
                if (!double.TryParse(
                        root.GetProperty("format").GetProperty("duration").GetString(),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out var duration)
                    || duration <= 0 || width <= 0 || height <= 0)
                {
                    return null;
                }
                return new VideoProbe(duration, width, height, hasAudio);
            }
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
        }
        return null;
    }

    private static int Rotation(JsonElement stream)
    {
        if (stream.TryGetProperty("side_data_list", out var sideData))
        {
            foreach (var item in sideData.EnumerateArray())
            {
                if (item.TryGetProperty("rotation", out var rotation) && rotation.TryGetInt32(out var value))
                    return value;
            }
        }
        if (stream.TryGetProperty("tags", out var tags)
            && tags.TryGetProperty("rotate", out var rotate)
            && int.TryParse(rotate.GetString(), out var legacy))
        {
            return legacy;
        }
        return 0;
    }

    private static ProcessStartInfo NewProcess(string fileName, IEnumerable<string> arguments)
    {
        var info = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        return info;
    }

    // Runs a child process, feeding each stdout line to the callback and keeping the stderr tail.
    private static async Task RunAsync(
        string fileName, IEnumerable<string> arguments, Action<string> onOutputLine,
        CancellationToken cancellationToken)
    {
        using var process = Process.Start(NewProcess(fileName, arguments))
            ?? throw new InvalidOperationException($"{fileName} could not be started.");
        try { process.PriorityClass = ProcessPriorityClass.BelowNormal; } catch (InvalidOperationException) { }

        var tail = new Queue<string>();
        var output = Task.Run(async () =>
        {
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line) onOutputLine(line);
        }, cancellationToken);
        var errors = Task.Run(async () =>
        {
            while (await process.StandardError.ReadLineAsync(cancellationToken) is { } line)
            {
                tail.Enqueue(line);
                if (tail.Count > 20) tail.Dequeue();
            }
        }, cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(output, errors);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{Path.GetFileName(fileName)} exited with code {process.ExitCode}: {string.Join(" | ", tail)}");
        }
    }
}
