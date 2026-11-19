using CsCompressor.Core.Models;
using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class BaselineCodecTests
{
    [Theory]
    [InlineData("libx264", VideoCodec.Libx264)]
    [InlineData("libx265", VideoCodec.Libx265)]
    [InlineData("libaom_av1", VideoCodec.LibaomAv1)]
    [InlineData("libaom-av1", VideoCodec.LibaomAv1)]
    [InlineData("libvpx_vp9", VideoCodec.LibvpxVp9)]
    [InlineData("libvpx-vp9", VideoCodec.LibvpxVp9)]
    [InlineData("h264_nvenc", VideoCodec.H264Nvenc)]
    [InlineData("copy", VideoCodec.Copy)]
    [InlineData("No encoding", VideoCodec.Copy)]
    [InlineData("gif", VideoCodec.Gif)]
    public void ParseVideoCodec_ValidNames_ReturnsExpected(string input, VideoCodec expected)
    {
        var result = CodecHelper.ParseVideoCodec(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("aac", AudioCodec.Aac)]
    [InlineData("flac", AudioCodec.Flac)]
    [InlineData("libmp3lame", AudioCodec.Libmp3lame)]
    [InlineData("mp3", AudioCodec.Libmp3lame)]
    [InlineData("opus", AudioCodec.Libopus)]
    [InlineData("copy", AudioCodec.Copy)]
    public void ParseAudioCodec_ValidNames_ReturnsExpected(string input, AudioCodec expected)
    {
        var result = CodecHelper.ParseAudioCodec(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MP4", ContainerFormat.Mp4)]
    [InlineData("MKV", ContainerFormat.Mkv)]
    [InlineData("WebM", ContainerFormat.Webm)]
    [InlineData("GIF", ContainerFormat.Gif)]
    [InlineData("MP3", ContainerFormat.Mp3)]
    public void ParseContainerFormat_ValidNames_ReturnsExpected(string input, ContainerFormat expected)
    {
        var result = CodecHelper.ParseContainerFormat(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void VideoCompatibility_Mp4WithLibvpx_ReturnsFalse()
    {
        Assert.False(CodecHelper.IsVideoCompatible(VideoCodec.Libvpx, ContainerFormat.Mp4));
    }

    [Fact]
    public void VideoCompatibility_Mp4WithLibx264_ReturnsTrue()
    {
        Assert.True(CodecHelper.IsVideoCompatible(VideoCodec.Libx264, ContainerFormat.Mp4));
    }

    [Fact]
    public void VideoCompatibility_WebmWithLibvpxVp9_ReturnsTrue()
    {
        Assert.True(CodecHelper.IsVideoCompatible(VideoCodec.LibvpxVp9, ContainerFormat.Webm));
    }

    [Fact]
    public void VideoCompatibility_WebmWithLibx264_ReturnsFalse()
    {
        Assert.False(CodecHelper.IsVideoCompatible(VideoCodec.Libx264, ContainerFormat.Webm));
    }

    [Fact]
    public void VideoCompatibility_Mkv_SupportsMostEncodersExceptGif()
    {
        Assert.True(CodecHelper.IsVideoCompatible(VideoCodec.Libx264, ContainerFormat.Mkv));
        Assert.True(CodecHelper.IsVideoCompatible(VideoCodec.Libx265, ContainerFormat.Mkv));
        Assert.True(CodecHelper.IsVideoCompatible(VideoCodec.LibaomAv1, ContainerFormat.Mkv));
        Assert.False(CodecHelper.IsVideoCompatible(VideoCodec.Gif, ContainerFormat.Mkv));
    }

    [Fact]
    public void AudioCompatibility_Mp4WithAac_ReturnsTrue()
    {
        Assert.True(CodecHelper.IsAudioCompatible(AudioCodec.Aac, ContainerFormat.Mp4));
    }

    [Fact]
    public void AudioCompatibility_Mp4WithSpeex_ReturnsFalse()
    {
        Assert.False(CodecHelper.IsAudioCompatible(AudioCodec.Libspeex, ContainerFormat.Mp4));
    }

    [Fact]
    public void AudioCompatibility_WebmWithOpus_ReturnsTrue()
    {
        Assert.True(CodecHelper.IsAudioCompatible(AudioCodec.Libopus, ContainerFormat.Webm));
    }

    [Fact]
    public void Presets_Nvenc_ReturnsP1ThroughP7()
    {
        var presets = CodecHelper.GetSupportedPresets(VideoCodec.H264Nvenc);
        Assert.Contains(EncoderPreset.P1, presets);
        Assert.Contains(EncoderPreset.P7, presets);
        Assert.DoesNotContain(EncoderPreset.Ultrafast, presets);
    }

    [Fact]
    public void Presets_X264_ReturnsUltrafastThroughPlacebo()
    {
        var presets = CodecHelper.GetSupportedPresets(VideoCodec.Libx264);
        Assert.Contains(EncoderPreset.Ultrafast, presets);
        Assert.Contains(EncoderPreset.Medium, presets);
        Assert.Contains(EncoderPreset.Placebo, presets);
        Assert.DoesNotContain(EncoderPreset.P1, presets);
    }
}
