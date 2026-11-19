using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class FFmpegProgressParserTests
{
    [Fact]
    public void ProcessLine_ProgressPipeOutput_CalculatesCorrectPercentageAndEta()
    {
        var totalDuration = TimeSpan.FromSeconds(100);
        var parser = new FFmpegProgressParser(totalDuration);

        parser.ProcessLine("frame=1500");
        parser.ProcessLine("fps=60.0");
        parser.ProcessLine("out_time_us=50000000"); // 50 seconds
        parser.ProcessLine("speed=2.0x");
        parser.ProcessLine("bitrate=1500.0kbits/s");
        var snapshot = parser.ProcessLine("progress=continue");

        Assert.NotNull(snapshot);
        Assert.Equal(1500, snapshot.Frame);
        Assert.Equal(60.0, snapshot.Fps);
        Assert.Equal(TimeSpan.FromSeconds(50), snapshot.CurrentTime);
        Assert.Equal(50.0, snapshot.Percentage);
        Assert.Equal(2.0, snapshot.Speed);
        Assert.Equal("1500.0kbits/s", snapshot.Bitrate);
        Assert.False(snapshot.IsComplete);

        // Remaining 50s at 2.0x speed = 25s ETA
        Assert.NotNull(snapshot.EstimatedTimeRemaining);
        Assert.Equal(25, (int)snapshot.EstimatedTimeRemaining.Value.TotalSeconds);
    }

    [Fact]
    public void ProcessLine_ProgressEnd_SetsCompleteAnd100Percent()
    {
        var totalDuration = TimeSpan.FromSeconds(60);
        var parser = new FFmpegProgressParser(totalDuration);

        parser.ProcessLine("out_time_us=60000000");
        var snapshot = parser.ProcessLine("progress=end");

        Assert.NotNull(snapshot);
        Assert.True(snapshot.IsComplete);
        Assert.Equal(100.0, snapshot.Percentage);
        Assert.Equal(TimeSpan.Zero, snapshot.EstimatedTimeRemaining);
    }

    [Fact]
    public void ProcessLine_StderrLine_ParsesCorrectly()
    {
        var totalDuration = TimeSpan.FromSeconds(60);
        var parser = new FFmpegProgressParser(totalDuration);

        var stderrLine = "frame=  300 fps= 59.8 q=28.0 size=    2048KiB time=00:00:10.00 bitrate=1677.7kbits/s speed=1.99x";
        var snapshot = parser.ProcessLine(stderrLine);

        Assert.NotNull(snapshot);
        Assert.Equal(300, snapshot.Frame);
        Assert.Equal(59.8, snapshot.Fps);
        Assert.Equal(TimeSpan.FromSeconds(10), snapshot.CurrentTime);
        Assert.Equal(16.7, Math.Round(snapshot.Percentage, 1));
        Assert.Equal(1.99, snapshot.Speed);
    }
}
