using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class ProcessRunnerTests
{
    [Fact]
    public async Task ExecuteAsync_ValidCommand_ReturnsZeroExitCodeAndOutput()
    {
        var ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        if (ffmpegPath == null) return;

        var result = await ProcessRunner.ExecuteAsync(ffmpegPath, "-version");

        Assert.Equal(0, result.ExitCode);
        Assert.True(result.IsSuccess);
        Assert.False(result.WasCancelled);
        Assert.Contains("ffmpeg version", result.StandardOutput);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidArguments_ReturnsNonZeroExitCodeAndErrorDetails()
    {
        var ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        if (ffmpegPath == null) return;

        var result = await ProcessRunner.ExecuteAsync(ffmpegPath, "-invalid_arg_123_test");

        Assert.NotEqual(0, result.ExitCode);
        Assert.False(result.IsSuccess);
        Assert.False(result.WasCancelled);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardError));
    }

    [Fact]
    public async Task ExecuteAsync_Cancellation_AbortsProcessPromptly()
    {
        var ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        if (ffmpegPath == null) return;

        using var cts = new CancellationTokenSource();
        var startedTcs = new TaskCompletionSource<bool>();

        var runTask = ProcessRunner.ExecuteAsync(
            ffmpegPath,
            "-re -f lavfi -i testsrc=duration=3600:size=320x240:rate=30 -f null -",
            onErrorLine: _ => startedTcs.TrySetResult(true),
            cancellationToken: cts.Token
        );

        // Wait until ffmpeg is actively executing
        await startedTcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cts.Cancel();

        var result = await runTask;

        Assert.True(result.WasCancelled);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_StreamingOutput_ReceivesCallbackLines()
    {
        var ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        if (ffmpegPath == null) return;

        var outputLines = new List<string>();
        var result = await ProcessRunner.ExecuteAsync(
            ffmpegPath,
            "-version",
            onOutputLine: line => outputLines.Add(line)
        );

        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(outputLines);
    }

    [Fact]
    public void ProcessAPI_Start_LegacySignature_ReturnsExpectedArray()
    {
        var ffmpegPath = FFmpegLocator.FindExecutable("ffmpeg");
        if (ffmpegPath == null) return;

        var result = ProcessAPI.Start(ffmpegPath, "-version");

        Assert.Equal(3, result.Length);
        Assert.Contains("ffmpeg version", result[0]); // stdout
        Assert.Equal("0", result[1]); // exit code
    }
}
