using CsCompressor.Core.Models;
using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class FFmpegIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string? _ffmpegPath;
    private readonly string? _ffprobePath;

    public FFmpegIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "CsCompressor_IntegrationTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        _ffprobePath = FFmpegLocator.FindExecutable("ffprobe");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Ignore temp dir cleanup errors
        }
    }

    private async Task<string> CreateSyntheticMediaAsync(string fileName, int durationSeconds = 3, int width = 640, int height = 360)
    {
        Assert.NotNull(_ffmpegPath);
        var outputPath = Path.Combine(_tempDir, fileName);

        // Generate synthetic video and audio stream
        var args = $"-y -f lavfi -i testsrc=duration={durationSeconds}:size={width}x{height}:rate=30 -f lavfi -i sine=frequency=1000:duration={durationSeconds} -c:v libx264 -c:a aac -pix_fmt yuv420p \"{outputPath}\"";
        var result = await ProcessRunner.ExecuteAsync(_ffmpegPath, args);

        Assert.True(result.IsSuccess, $"Failed to create synthetic media: {result.StandardError}");
        Assert.True(File.Exists(outputPath), $"Synthetic media was not created: {outputPath}");
        return outputPath;
    }

    [Fact]
    public async Task Transcode_Mp4ToMp4_ProducesValidCompressedVideo()
    {
        if (_ffmpegPath == null) return;

        var inputPath = await CreateSyntheticMediaAsync("source_transcode.mp4", durationSeconds: 2);
        var outputPath = Path.Combine(_tempDir, "output_transcode.mp4");

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            AudioCodec = AudioCodec.Aac,
            RateControl = RateControlMode.Cq,
            CQConstant = "28",
            TargetWidth = 320,
            TargetHeight = 180,
            Framerate = 15.0,
            EnableProgressReporting = true
        };

        var progressSnapshots = new List<CompressionProgress>();
        var duration = TimeSpan.FromSeconds(2);
        var parser = new FFmpegProgressParser(duration);

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var result = await ProcessRunner.ExecuteAsync(
            _ffmpegPath,
            argumentList: args,
            onOutputLine: line =>
            {
                var p = parser.ProcessLine(line);
                if (p != null) progressSnapshots.Add(p);
            }
        );

        Assert.True(result.IsSuccess, $"Compression failed: {result.StandardError}");
        Assert.True(File.Exists(outputPath), "Output file was not generated.");
        Assert.True(new FileInfo(outputPath).Length > 0, "Output file is empty.");

        // Verify with probe service
        var probe = await MediaProbeService.ProbeAsync(outputPath, _ffprobePath);
        Assert.NotNull(probe);
        Assert.Equal(320, probe.Width);
        Assert.Equal(180, probe.Height);
        Assert.Equal("h264", probe.VideoCodec);
        Assert.Equal("aac", probe.AudioCodec);
        Assert.NotEmpty(progressSnapshots);
    }

    [Fact]
    public async Task Transcode_ToWebm_ProducesValidVp9Video()
    {
        if (_ffmpegPath == null) return;

        var inputPath = await CreateSyntheticMediaAsync("source_webm.mp4", durationSeconds: 2);
        var outputPath = Path.Combine(_tempDir, "output_webm.webm");

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Webm,
            VideoCodec = VideoCodec.LibvpxVp9,
            AudioCodec = AudioCodec.Libopus,
            RateControl = RateControlMode.Cq,
            CQConstant = "35",
            Preset = EncoderPreset.Realtime,
            CpuUsed = 4,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var result = await ProcessRunner.ExecuteAsync(_ffmpegPath, argumentList: args);

        Assert.True(result.IsSuccess, $"WebM transcode failed: {result.StandardError}");
        Assert.True(File.Exists(outputPath));

        var probe = await MediaProbeService.ProbeAsync(outputPath, _ffprobePath);
        Assert.NotNull(probe);
        Assert.Equal("vp9", probe.VideoCodec);
        Assert.Equal("opus", probe.AudioCodec);
    }

    [Fact]
    public async Task Transcode_ToGifWithAccurateColor_ProducesValidGif()
    {
        if (_ffmpegPath == null) return;

        var inputPath = await CreateSyntheticMediaAsync("source_gif.mp4", durationSeconds: 1);
        var outputPath = Path.Combine(_tempDir, "output_animated.gif");

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Gif,
            VideoCodec = VideoCodec.Gif,
            TargetWidth = 160,
            TargetHeight = 90,
            Framerate = 10.0,
            AccurateColorGif = true,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var result = await ProcessRunner.ExecuteAsync(_ffmpegPath, argumentList: args);

        Assert.True(result.IsSuccess, $"GIF generation failed: {result.StandardError}");
        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }

    [Fact]
    public async Task Transcode_WithTrimming_ProducesTrimmedDuration()
    {
        if (_ffmpegPath == null) return;

        // Source is 6 seconds
        var inputPath = await CreateSyntheticMediaAsync("source_trim.mp4", durationSeconds: 6);
        var outputPath = Path.Combine(_tempDir, "output_trim.mp4");

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            StartTime = TimeSpan.FromSeconds(2),
            EndTime = TimeSpan.FromSeconds(5), // 3 seconds duration
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var result = await ProcessRunner.ExecuteAsync(_ffmpegPath, argumentList: args);

        Assert.True(result.IsSuccess, $"Trim failed: {result.StandardError}");
        Assert.True(File.Exists(outputPath));

        var probe = await MediaProbeService.ProbeAsync(outputPath, _ffprobePath);
        Assert.NotNull(probe);
        Assert.InRange(probe.Duration.TotalSeconds, 2.7, 3.5);
    }

    [Fact]
    public async Task Transcode_PathWithSpacesAndSpecialCharacters_HandledCorrectly()
    {
        if (_ffmpegPath == null) return;

        var inputFileName = "my video (sample #1) with spaces.mp4";
        var outputFileName = "compressed (final #2) with spaces.mp4";

        var inputPath = await CreateSyntheticMediaAsync(inputFileName, durationSeconds: 1);
        var outputPath = Path.Combine(_tempDir, outputFileName);

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var result = await ProcessRunner.ExecuteAsync(_ffmpegPath, argumentList: args);

        Assert.True(result.IsSuccess, $"Handling special characters failed: {result.StandardError}");
        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }

    [Fact]
    public async Task Transcode_CancellationMidFlight_TerminatesCleanly()
    {
        if (_ffmpegPath == null) return;

        var inputPath = await CreateSyntheticMediaAsync("source_cancel.mp4", durationSeconds: 10);
        var outputPath = Path.Combine(_tempDir, "output_cancelled.mp4");

        var options = new CompressionOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            Preset = EncoderPreset.Veryslow, // Slower encode so we have plenty of time to cancel
            EnableProgressReporting = true
        };

        using var cts = new CancellationTokenSource();
        var progressStartedTcs = new TaskCompletionSource<bool>();
        var parser = new FFmpegProgressParser(TimeSpan.FromSeconds(10));

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);
        var runTask = ProcessRunner.ExecuteAsync(
            _ffmpegPath,
            argumentList: args,
            onOutputLine: line =>
            {
                var p = parser.ProcessLine(line);
                if (p != null && p.Frame > 5)
                {
                    progressStartedTcs.TrySetResult(true);
                }
            },
            cancellationToken: cts.Token
        );

        // Wait until transcoding has actively started
        await progressStartedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cts.Cancel();

        var result = await runTask;

        Assert.True(result.WasCancelled);
        Assert.False(result.IsSuccess);
    }
}
