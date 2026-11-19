using System.IO;
using System.Diagnostics;
using System.Windows;
using VideoProcessor;
using CsCompressor.Core.Services;

public class FFmpegAPI
{
    public static string? ffmpegPath;
    public static string? ffprobePath;
    public static string? magickPath;
    public static string? testVideoPath;
    public static string[] availableVideoEncoders = { "libx264", "libx265", "libaom_av1", "libvpx", "libvpx_vp9", "gif" };
    public static VideoProcessingWindow? currentVIdeoProcessingWindow;

    public static bool CheckFFmpeg(bool showDialog = true)
    {
        if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
        {
            if (showDialog)
            {
                MessageBoxAPI.Error("FFmpeg executable not found. Please install FFmpeg on your system or place it in the application folder.", Crash: false);
            }
            return false;
        }
        return true;
    }

    private static bool TestEncoder(string encoder)
    {
        if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath)) return false;

        if (Enum.TryParse<FFmpegArg.supportedVideoEncoders>(encoder, out FFmpegArg.supportedVideoEncoders a))
        {
            string nullDevice = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
            FFmpegArg arg = new FFmpegArg()
            {
                ffprobePath = ffprobePath ?? ffmpegPath,
                inputPath = "testsrc=d=0.1",
                isTestFile = true,
                loggingMode = "quiet",
                videoEncoder = a,
                outputFormat = FFmpegArg.supportedOutputFormats.MKV,
                outputPath = nullDevice
            };

            string? argString = arg.ToString();
            if (argString == null) { return false; }

            string[] p = ProcessAPI.Start(ffmpegPath, argString);
            if (p[1] == "0")
            {
                if (!availableVideoEncoders.Contains(encoder))
                {
                    availableVideoEncoders = [.. availableVideoEncoders, encoder];
                }
                return true;
            }
            return false;
        }
        return false;
    }

    public static async Task CheckAllEncoders(VideoProcessingWindow aa)
    {
        await Task.Run(() =>
        {
            TestEncoder("h264_nvenc"); TestEncoder("hevc_nvenc"); TestEncoder("av1_nvenc");
            TestEncoder("h264_amf"); TestEncoder("hevc_amf"); TestEncoder("av1_amf");
            TestEncoder("h264_qsv"); TestEncoder("hevc_qsv"); TestEncoder("av1_qsv");
        });

        if (Application.Current != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                aa.UpdateVideoEncoders(availableVideoEncoders);
            });
        }
        else
        {
            aa.UpdateVideoEncoders(availableVideoEncoders);
        }
    }

    public static void Init()
    {
        // 1. Locate system or local FFmpeg and FFprobe
        var ffmpegResult = FFmpegLocator.ValidateBinary("ffmpeg");
        if (ffmpegResult.IsAvailable)
        {
            ffmpegPath = ffmpegResult.ExecutablePath;
        }

        var ffprobeResult = FFmpegLocator.ValidateBinary("ffprobe");
        if (ffprobeResult.IsAvailable)
        {
            ffprobePath = ffprobeResult.ExecutablePath;
        }

        // 2. If not found and on Windows, fallback to bundled archive extraction
        if (string.IsNullOrEmpty(ffmpegPath) && OperatingSystem.IsWindows())
        {
            Extract.GetPath("7za.exe");
            ffmpegPath = Extract.GetPath("ffmpeg.7z", "ffmpeg.exe");
        }

        if (string.IsNullOrEmpty(ffprobePath) && OperatingSystem.IsWindows())
        {
            ffprobePath = Extract.GetPath("ffprobe.7z", "ffprobe.exe");
        }

        CheckFFmpeg(showDialog: false);
    }
}
