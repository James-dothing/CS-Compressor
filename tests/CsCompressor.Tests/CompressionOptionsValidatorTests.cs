using CsCompressor.Core.Models;
using CsCompressor.Core.Validation;
using Xunit;

namespace CsCompressor.Tests;

public class CompressionOptionsValidatorTests
{
    [Fact]
    public void Validate_MissingInputPath_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "",
            OutputPath = "output.mp4"
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.InputPath));
    }

    [Fact]
    public void Validate_MissingOutputPath_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = ""
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.OutputPath));
    }

    [Fact]
    public void Validate_SameInputAndOutputPath_ReturnsError()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "video data");
            var options = new CompressionOptions
            {
                InputPath = tempFile,
                OutputPath = tempFile
            };

            var result = CompressionOptionsValidator.Validate(options);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.OutputPath));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Validate_IncompatibleVideoCodec_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.webm",
            OutputFormat = ContainerFormat.Webm,
            VideoCodec = VideoCodec.Libx264 // WebM doesn't take x264 natively
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.VideoCodec));
    }

    [Fact]
    public void Validate_CbrMissingBitrate_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            RateControl = RateControlMode.Cbr,
            VideoBitrate = ""
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.VideoBitrate));
    }

    [Fact]
    public void Validate_CqInvalidRange_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            RateControl = RateControlMode.Cq,
            CQConstant = "105" // Way out of range (0-51 / 0-63)
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.CQConstant));
    }

    [Fact]
    public void Validate_TrimmingStartAfterEnd_ReturnsError()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            StartTime = TimeSpan.FromSeconds(30),
            EndTime = TimeSpan.FromSeconds(10)
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(options.StartTime));
    }

    [Fact]
    public void Validate_OddDimensions_AddsWarning()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            TargetWidth = 1920,
            TargetHeight = 1081 // Odd height
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.True(result.IsValid); // Warning, not a fatal validation error
        Assert.Contains(result.Warnings, w => w.PropertyName == nameof(options.TargetWidth));
    }

    [Fact]
    public void Validate_ValidConfiguration_PassesValidation()
    {
        var options = new CompressionOptions
        {
            InputPath = "testsrc=d=1",
            IsTestFile = true,
            OutputPath = "output.mp4",
            OutputFormat = ContainerFormat.Mp4,
            VideoCodec = VideoCodec.Libx264,
            AudioCodec = AudioCodec.Aac,
            RateControl = RateControlMode.Cq,
            CQConstant = "23",
            TargetWidth = 1920,
            TargetHeight = 1080,
            Framerate = 60.0
        };

        var result = CompressionOptionsValidator.Validate(options);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
