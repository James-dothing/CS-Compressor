using CsCompressor.Core.Models;
using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class FFmpegArgumentBuilderTests
{
    [Fact]
    public void BuildArgumentList_BasicTranscode_ContainsExpectedArgs()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            AudioCodec = AudioCodec.Aac,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-y", args);
        Assert.Contains("-i", args);
        Assert.Contains("input.mp4", args);
        Assert.Contains("-c:v", args);
        Assert.Contains("libx264", args);
        Assert.Contains("-c:a", args);
        Assert.Contains("aac", args);
        Assert.Contains("-f", args);
        Assert.Contains("mp4", args);
        Assert.Contains("output.mp4", args);
    }

    [Fact]
    public void BuildArgumentList_WithFilters_MapsOutvAndDoesNotDuplicateRawVideoStream()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            TargetWidth = 1280,
            TargetHeight = 720,
            Framerate = 30.0,
            ScalingAlgorithm = ScalingAlgorithm.Lanczos,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-filter_complex", args);
        Assert.Contains("[outv]", args);

        // Crucial fix: raw video stream 0:v:0? MUST NOT be mapped when filters produce [outv]
        Assert.DoesNotContain("0:v:0?", args);
    }

    [Fact]
    public void BuildArgumentList_WithoutFilters_MapsRawVideoStream()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-map", args);
        Assert.Contains("0:v:0?", args);
        Assert.DoesNotContain("-filter_complex", args);
    }

    [Fact]
    public void BuildArgumentList_CbrRateControl_GeneratesMinMaxRate()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            RateControl = RateControlMode.Cbr,
            VideoBitrate = "2000k",
            BufferSize = "4000k",
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-b:v", args);
        Assert.Contains("2000k", args);
        Assert.Contains("-minrate", args);
        Assert.Contains("-maxrate", args);
        Assert.Contains("-bufsize", args);
        Assert.Contains("4000k", args);
    }

    [Fact]
    public void BuildArgumentList_CqRateControl_GeneratesCrf()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            RateControl = RateControlMode.Cq,
            CQConstant = "23",
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-crf", args);
        Assert.Contains("23", args);
    }

    [Fact]
    public void BuildArgumentList_VpxPreset_GeneratesDeadline()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.webm",
            OutputPath = "output.webm",
            OutputFormat = ContainerFormat.Webm,
            VideoCodec = VideoCodec.LibvpxVp9,
            Preset = EncoderPreset.Good,
            CpuUsed = 2,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-deadline", args);
        Assert.Contains("good", args);
        Assert.Contains("-cpu-used", args);
        Assert.Contains("2", args);
    }

    [Fact]
    public void BuildArgumentList_DisableAudio_EmitsAnFlag()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            DisableAudio = true,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-an", args);
        Assert.DoesNotContain("-c:a", args);
    }

    [Fact]
    public void BuildArgumentList_AccurateColorGif_GeneratesPaletteFilters()
    {
        var options = new CompressionOptions
        {
            InputPath = "input.mp4",
            OutputPath = "output.gif",
            OutputFormat = ContainerFormat.Gif,
            VideoCodec = VideoCodec.Gif,
            AccurateColorGif = true,
            EnableProgressReporting = false
        };

        var args = FFmpegArgumentBuilder.BuildArgumentList(options);

        Assert.Contains("-filter_complex", args);
        var filterIndex = args.ToList().IndexOf("-filter_complex");
        var filterStr = args[filterIndex + 1];

        Assert.Contains("palettegen", filterStr);
        Assert.Contains("paletteuse", filterStr);
        Assert.Contains("-an", args); // GIFs do not have audio
    }

    [Fact]
    public void EscapeArgument_PathsWithSpaces_WrapsInQuotes()
    {
        var path = "my folder/cool video file (2026).mp4";
        var escaped = FFmpegArgumentBuilder.EscapeArgument(path);
        Assert.Equal("\"my folder/cool video file (2026).mp4\"", escaped);
    }

    [Fact]
    public void BuildCommandLine_SpecialPathCharacters_EscapesCorrectly()
    {
        var options = new CompressionOptions
        {
            InputPath = "C:\\Videos\\Clip & Edit.mp4",
            OutputPath = "C:\\Videos\\Final Output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            EnableProgressReporting = false
        };

        var cmd = FFmpegArgumentBuilder.BuildCommandLine(options);

        Assert.Contains("\"C:\\Videos\\Clip & Edit.mp4\"", cmd);
        Assert.Contains("\"C:\\Videos\\Final Output.mp4\"", cmd);
    }
}
