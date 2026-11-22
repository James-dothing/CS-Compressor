// most of the ffmpeg trickery are in /FFmpegArgumentMaker.cs
using System.IO;
using System.Diagnostics;
using CsCompressor;

public class FFmpegAPI
{
    public static String? ffmpegPath;
    public static String? ffprobePath;
    public static String[] availableVideoEncoders = {"libx264", "libx265", "libaom_av1", "libvpx", "libvpx_vp9","gif"};
    public static Process currentRunningAsyncProcess = new Process() {};
    public static VideoProcessingWindow? currentVIdeoProcessingWindow;
    public static async Task Start(string argument, bool noWindow, double dur)
    {
        try
        {
            Process? p = new Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = " " + argument;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = !noWindow;
            p.Start();
            if (p==null) {return;}
            currentRunningAsyncProcess = (Process)p;
            ProgressWindow progressWindow = new ProgressWindow() {};
            progressWindow.Show();
            while (true)
            {
                string? line = await p.StandardOutput.ReadLineAsync();
                if (line == null) {break;}
                line = line.Trim();
                if (line.Split("=").Length > 1)
                {
                    if (line.Split("=")[0] == "out_time_us")
                    {
                        double progressDur = (double)QOL.StringToDouble(line.Split("=")[1])! / 1000000.0d;
                        double percent = (progressDur/dur) * 100.0d;
                        _ = progressWindow.SetPercentageAsync(percent);
                    }
                }
            }
            _ = progressWindow.SetPercentageAsync(100.0d);
            string err = await p.StandardError.ReadToEndAsync();
            // string output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();
            int exitCode = p.ExitCode;
            if (exitCode != 0)
            {
                MessageBoxAPI.Error($"the process did not exit correctly\nthis can either be the program aborted or some issue with the program\n{err}");
            }
            return;
        } catch (FileNotFoundException)
        {
            return;
        }
    }
    public static void Abort()
    {
        if (currentRunningAsyncProcess != null && !currentRunningAsyncProcess.HasExited)
        {
            try
            {
                currentRunningAsyncProcess.Kill();
            }
            catch (InvalidOperationException) {}
        }
    }
    public static void CheckFFmpeg()
    {
        if (ffmpegPath != null)
        {if (!File.Exists(ffmpegPath)) {MessageBoxAPI.Error("FFmpeg not found");}}
        else // ffmpegPath is null
        {MessageBoxAPI.Error("FFmpeg path is NULL");}
    }

    private static bool TestEncoder(string encoder)
    {
        if (Enum.TryParse<FFmpegArg.supportedVideoEncoders>(encoder, out FFmpegArg.supportedVideoEncoders a))
        {
            FFmpegArg arg = new FFmpegArg() 
            {
                ffprobePath=ffprobePath, inputPath="testsrc=d=0.1", isTestFile=true, loggingMode="quiet",
                videoEncoder=a, outputFormat=FFmpegArg.supportedOutputFormats.MKV, outputPath="NUL"
            };
            string? argString = arg.ToString();
            if (argString==null) {return false;}
            string[] p = ProcessAPI.Start(ffmpegPath!, argString);
            if (p[1] == "0")
            {
                // MessageBoxAPI.Debug(string.IsNullOrEmpty(p[2]) ? "empty" : p[2]);
                availableVideoEncoders = [..availableVideoEncoders, encoder];
                return true;
            } else
            {
                return false;
            }
        } else
        {
            MessageBoxAPI.Error("Something went wrong!\nT.E+" + encoder);
        }
        return false;
    }
    async public static Task CheckAllEncoders(VideoProcessingWindow aa)
    {
        await Task.Run(() =>
        {
            TestEncoder("h264_nvenc"); TestEncoder("hevc_nvenc"); TestEncoder("av1_nvenc");
            TestEncoder("h264_amf"); TestEncoder("hevc_amf"); TestEncoder("av1_amf");
            TestEncoder("h264_qsv"); TestEncoder("hevc_qsv"); TestEncoder("av1_qsv");
        });

        aa.UpdateVideoEncoders(availableVideoEncoders);
    }

    public static void Init()
    {
        Extract.GetPath("7za.exe");
        ffmpegPath = Extract.GetPath("ffmpeg.7z", "ffmpeg.exe");
        ffprobePath= Extract.GetPath("ffprobe.7z", "ffprobe.exe");
        CheckFFmpeg();

    }
    //
}
