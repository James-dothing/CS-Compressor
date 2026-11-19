using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class FFmpegLocatorTests
{
    [Fact]
    public void ExtractVersionString_StandardFFmpegOutput_ReturnsVersion()
    {
        var output = "ffmpeg version 8.1.3 Copyright (c) 2000-2026 the FFmpeg developers\nbuilt with gcc 16";
        var version = FFmpegLocator.ExtractVersionString(output);
        Assert.Equal("8.1.3", version);
    }

    [Fact]
    public void ExtractVersionString_GitBuildOutput_ReturnsVersion()
    {
        var output = "ffmpeg version n6.1.1-2-g1234567 Copyright (c) 2000-2024 the FFmpeg developers";
        var version = FFmpegLocator.ExtractVersionString(output);
        Assert.Equal("n6.1.1-2-g1234567", version);
    }

    [Fact]
    public void ExtractVersionString_FFprobeOutput_ReturnsVersion()
    {
        var output = "ffprobe version 7.0-full_build-www.gyan.dev Copyright (c) 2007-2024";
        var version = FFmpegLocator.ExtractVersionString(output);
        Assert.Equal("7.0-full_build-www.gyan.dev", version);
    }

    [Fact]
    public void ExtractVersionString_EmptyOutput_ReturnsNull()
    {
        var version = FFmpegLocator.ExtractVersionString("");
        Assert.Null(version);
    }

    [Fact]
    public void ValidateBinary_NonExistentBinary_ReturnsNotAvailable()
    {
        var result = FFmpegLocator.ValidateBinary("non_existent_binary_xyz_12345");
        Assert.False(result.IsAvailable);
        Assert.Null(result.ExecutablePath);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void FindExecutable_WithFFmpegOnHost_FindsBinary()
    {
        // When running in an environment where ffmpeg is installed
        var path = FFmpegLocator.FindExecutable("ffmpeg");
        if (File.Exists("/usr/bin/ffmpeg"))
        {
            Assert.NotNull(path);
            Assert.True(File.Exists(path));
        }
    }

    [Fact]
    public void ValidateBinary_WithFFmpegOnHost_ReturnsAvailable()
    {
        if (File.Exists("/usr/bin/ffmpeg"))
        {
            var result = FFmpegLocator.ValidateBinary("ffmpeg");
            Assert.True(result.IsAvailable);
            Assert.NotNull(result.ExecutablePath);
            Assert.NotNull(result.Version);
            Assert.Null(result.ErrorMessage);
        }
    }
}
