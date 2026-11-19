using System.Diagnostics;
using System.Globalization;
using CsCompressor.Core.Models;

namespace CsCompressor.Core.Services;

public static class MediaProbeService
{
    public static async Task<MediaInfo?> ProbeAsync(string filePath, string? ffprobePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        var probeExe = !string.IsNullOrWhiteSpace(ffprobePath) && File.Exists(ffprobePath)
            ? ffprobePath
            : FFmpegLocator.FindExecutable("ffprobe");

        long fileSize = 0;
        try
        {
            fileSize = new FileInfo(filePath).Length;
        }
        catch
        {
            // Ignore file size errors
        }

        if (string.IsNullOrEmpty(probeExe))
        {
            // Fallback: minimal info without ffprobe
            return new MediaInfo(
                FilePath: filePath,
                Duration: TimeSpan.Zero,
                VideoCodec: null,
                AudioCodec: null,
                Width: 0,
                Height: 0,
                Framerate: 0,
                FileSizeBytes: fileSize
            );
        }

        try
        {
            // Probe video stream and format
            var args = $"-v error -show_entries format=duration -show_entries stream=codec_name,width,height,r_frame_rate -of default=noprint_wrappers=1:nokey=0 \"{filePath}\"";
            var result = await ProcessRunner.ExecuteAsync(probeExe, args);

            TimeSpan duration = TimeSpan.Zero;
            string? videoCodec = null;
            string? audioCodec = null;
            int width = 0;
            int height = 0;
            double framerate = 0.0;

            if (result.IsSuccess)
            {
                var lines = result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;
                    var key = parts[0].Trim();
                    var val = parts[1].Trim();

                    switch (key)
                    {
                        case "duration":
                            if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var sec))
                            {
                                duration = TimeSpan.FromSeconds(sec);
                            }
                            break;
                        case "codec_name" when videoCodec == null:
                            videoCodec = val;
                            break;
                        case "width" when width == 0:
                            int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out width);
                            break;
                        case "height" when height == 0:
                            int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out height);
                            break;
                        case "r_frame_rate" when framerate == 0.0:
                            framerate = ParseFrameRate(val);
                            break;
                    }
                }
            }

            // Probe audio codec
            var audioArgs = $"-v error -select_streams a:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"";
            var audioResult = await ProcessRunner.ExecuteAsync(probeExe, audioArgs);
            if (audioResult.IsSuccess && !string.IsNullOrWhiteSpace(audioResult.StandardOutput))
            {
                audioCodec = audioResult.StandardOutput.Trim();
            }

            return new MediaInfo(
                FilePath: filePath,
                Duration: duration,
                VideoCodec: videoCodec,
                AudioCodec: audioCodec,
                Width: width,
                Height: height,
                Framerate: framerate,
                FileSizeBytes: fileSize
            );
        }
        catch
        {
            return new MediaInfo(
                FilePath: filePath,
                Duration: TimeSpan.Zero,
                VideoCodec: null,
                AudioCodec: null,
                Width: 0,
                Height: 0,
                Framerate: 0,
                FileSizeBytes: fileSize
            );
        }
    }

    private static double ParseFrameRate(string rateStr)
    {
        if (string.IsNullOrWhiteSpace(rateStr)) return 0.0;
        var parts = rateStr.Split('/');
        if (parts.Length == 2
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var num)
            && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var den)
            && den > 0)
        {
            return num / den;
        }

        if (double.TryParse(rateStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var direct))
        {
            return direct;
        }

        return 0.0;
    }
}
