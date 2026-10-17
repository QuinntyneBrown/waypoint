using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

/// <summary>Drives the self-hosted ffprobe and ffmpeg binaries as child processes.</summary>
public sealed class FfmpegVideoTranscoder : IVideoTranscoder
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(30);

    private readonly MediaOptions _options;

    public FfmpegVideoTranscoder(IOptions<MediaOptions> options)
    {
        _options = options.Value;
    }

    public async Task<VideoProbe?> ProbeAsync(string sourcePath, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo(_options.FfprobePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in new[]
        {
            "-v", "error", "-print_format", "json", "-show_format", "-show_streams", sourcePath,
        })
        {
            info.ArgumentList.Add(argument);
        }

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

    /// <summary>Reads duration and display size (after rotation) from ffprobe JSON.</summary>
    public static VideoProbe? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("streams", out var streams)) return null;
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
                return new VideoProbe(duration, width, height);
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
}
