using CsCompressor.Core.Models;

namespace CsCompressor.Core.Services;

public static class CodecHelper
{
    public static VideoCodec? ParseVideoCodec(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim().ToLowerInvariant().Replace('-', '_');

        return name switch
        {
            "copy" or "no encoding" => VideoCodec.Copy,
            "gif" => VideoCodec.Gif,
            "libx264" or "h264" or "x264" => VideoCodec.Libx264,
            "libx265" or "hevc" or "x265" => VideoCodec.Libx265,
            "libaom_av1" or "av1" => VideoCodec.LibaomAv1,
            "libvpx" or "vp8" => VideoCodec.Libvpx,
            "libvpx_vp9" or "vp9" => VideoCodec.LibvpxVp9,
            "h264_nvenc" => VideoCodec.H264Nvenc,
            "hevc_nvenc" => VideoCodec.HevcNvenc,
            "av1_nvenc" => VideoCodec.Av1Nvenc,
            "h264_amf" => VideoCodec.H264Amf,
            "hevc_amf" => VideoCodec.HevcAmf,
            "av1_amf" => VideoCodec.Av1Amf,
            "h264_qsv" => VideoCodec.H264Qsv,
            "hevc_qsv" => VideoCodec.HevcQsv,
            "av1_qsv" => VideoCodec.Av1Qsv,
            _ => null
        };
    }

    public static string ToFFmpegVideoCodecName(VideoCodec codec) => codec switch
    {
        VideoCodec.Copy => "copy",
        VideoCodec.Gif => "gif",
        VideoCodec.Libx264 => "libx264",
        VideoCodec.Libx265 => "libx265",
        VideoCodec.LibaomAv1 => "libaom-av1",
        VideoCodec.Libvpx => "libvpx",
        VideoCodec.LibvpxVp9 => "libvpx-vp9",
        VideoCodec.H264Nvenc => "h264_nvenc",
        VideoCodec.HevcNvenc => "hevc_nvenc",
        VideoCodec.Av1Nvenc => "av1_nvenc",
        VideoCodec.H264Amf => "h264_amf",
        VideoCodec.HevcAmf => "hevc_amf",
        VideoCodec.Av1Amf => "av1_amf",
        VideoCodec.H264Qsv => "h264_qsv",
        VideoCodec.HevcQsv => "hevc_qsv",
        VideoCodec.Av1Qsv => "av1_qsv",
        _ => codec.ToString().ToLowerInvariant()
    };

    public static AudioCodec? ParseAudioCodec(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim().ToLowerInvariant().Replace('-', '_');

        return name switch
        {
            "copy" or "no encoding" => AudioCodec.Copy,
            "aac" => AudioCodec.Aac,
            "flac" => AudioCodec.Flac,
            "libmp3lame" or "mp3" => AudioCodec.Libmp3lame,
            "libvorbis" or "vorbis" => AudioCodec.Libvorbis,
            "libopus" or "opus" => AudioCodec.Libopus,
            "libspeex" or "speex" => AudioCodec.Libspeex,
            "pcm_s16le" => AudioCodec.PcmS16le,
            "pcm_s24le" => AudioCodec.PcmS24le,
            "pcm_s32le" => AudioCodec.PcmS32le,
            "pcm_f32le" => AudioCodec.PcmF32le,
            "pcm_f64le" => AudioCodec.PcmF64le,
            "pcm_alaw" or "alaw" => AudioCodec.PcmAlaw,
            "alac" => AudioCodec.Alac,
            _ => null
        };
    }

    public static string ToFFmpegAudioCodecName(AudioCodec codec) => codec switch
    {
        AudioCodec.Copy => "copy",
        AudioCodec.Aac => "aac",
        AudioCodec.Flac => "flac",
        AudioCodec.Libmp3lame => "libmp3lame",
        AudioCodec.Libvorbis => "libvorbis",
        AudioCodec.Libopus => "libopus",
        AudioCodec.Libspeex => "libspeex",
        AudioCodec.PcmS16le => "pcm_s16le",
        AudioCodec.PcmS24le => "pcm_s24le",
        AudioCodec.PcmS32le => "pcm_s32le",
        AudioCodec.PcmF32le => "pcm_f32le",
        AudioCodec.PcmF64le => "pcm_f64le",
        AudioCodec.PcmAlaw => "pcm_alaw",
        AudioCodec.Alac => "alac",
        _ => codec.ToString().ToLowerInvariant()
    };

    public static ContainerFormat? ParseContainerFormat(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim().ToUpperInvariant();

        return name switch
        {
            "MP4" => ContainerFormat.Mp4,
            "MOV" => ContainerFormat.Mov,
            "GIF" => ContainerFormat.Gif,
            "MKV" => ContainerFormat.Mkv,
            "WEBM" => ContainerFormat.Webm,
            "OGG" => ContainerFormat.Ogg,
            "AAC" => ContainerFormat.Aac,
            "WAV" => ContainerFormat.Wav,
            "FLAC" => ContainerFormat.Flac,
            "MP3" => ContainerFormat.Mp3,
            _ => null
        };
    }

    public static string ToFormatExtension(ContainerFormat format) => format switch
    {
        ContainerFormat.Mp4 => "mp4",
        ContainerFormat.Mov => "mov",
        ContainerFormat.Gif => "gif",
        ContainerFormat.Mkv => "mkv",
        ContainerFormat.Webm => "webm",
        ContainerFormat.Ogg => "ogg",
        ContainerFormat.Aac => "aac",
        ContainerFormat.Wav => "wav",
        ContainerFormat.Flac => "flac",
        ContainerFormat.Mp3 => "mp3",
        _ => format.ToString().ToLowerInvariant()
    };

    public static string ToFFmpegFormatName(ContainerFormat format) => format switch
    {
        ContainerFormat.Mkv => "matroska",
        ContainerFormat.Aac => "adts",
        ContainerFormat.Mp4 => "mp4",
        ContainerFormat.Mov => "mov",
        ContainerFormat.Gif => "gif",
        ContainerFormat.Webm => "webm",
        ContainerFormat.Ogg => "ogg",
        ContainerFormat.Wav => "wav",
        ContainerFormat.Flac => "flac",
        ContainerFormat.Mp3 => "mp3",
        _ => format.ToString().ToLowerInvariant()
    };

    public static RateControlMode? ParseRateControlMode(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return name.Trim().ToUpperInvariant() switch
        {
            "CBR" => RateControlMode.Cbr,
            "VBR" => RateControlMode.Vbr,
            "CQP" => RateControlMode.Cqp,
            "CQ" or "CRF" => RateControlMode.Cq,
            _ => null
        };
    }

    public static EncoderPreset? ParsePreset(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (Enum.TryParse<EncoderPreset>(name.Trim(), true, out var preset))
        {
            return preset;
        }
        return null;
    }

    public static ScalingAlgorithm? ParseScalingAlgorithm(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim().ToLowerInvariant().Replace("_", "");
        return name switch
        {
            "fastbilinear" => ScalingAlgorithm.FastBilinear,
            "bilinear" => ScalingAlgorithm.Bilinear,
            "bicubic" => ScalingAlgorithm.Bicubic,
            "neighbor" => ScalingAlgorithm.Neighbor,
            "area" => ScalingAlgorithm.Area,
            "bicublin" => ScalingAlgorithm.Bicublin,
            "gauss" => ScalingAlgorithm.Gauss,
            "sinc" => ScalingAlgorithm.Sinc,
            "lanczos" => ScalingAlgorithm.Lanczos,
            "spline" => ScalingAlgorithm.Spline,
            _ => null
        };
    }

    public static string ToScalingAlgorithmName(ScalingAlgorithm algorithm) => algorithm switch
    {
        ScalingAlgorithm.FastBilinear => "fast_bilinear",
        ScalingAlgorithm.Bilinear => "bilinear",
        ScalingAlgorithm.Bicubic => "bicubic",
        ScalingAlgorithm.Neighbor => "neighbor",
        ScalingAlgorithm.Area => "area",
        ScalingAlgorithm.Bicublin => "bicublin",
        ScalingAlgorithm.Gauss => "gauss",
        ScalingAlgorithm.Sinc => "sinc",
        ScalingAlgorithm.Lanczos => "lanczos",
        ScalingAlgorithm.Spline => "spline",
        _ => algorithm.ToString().ToLowerInvariant()
    };

    public static bool IsAudioOnlyFormat(ContainerFormat format) =>
        format is ContainerFormat.Aac or ContainerFormat.Wav or ContainerFormat.Flac or ContainerFormat.Mp3 or ContainerFormat.Ogg;

    public static bool IsVideoCompatible(VideoCodec videoCodec, ContainerFormat format)
    {
        if (videoCodec == VideoCodec.Copy) return true;

        return format switch
        {
            ContainerFormat.Mp4 => videoCodec is not (VideoCodec.Libvpx or VideoCodec.LibvpxVp9 or VideoCodec.Gif),
            ContainerFormat.Mov => videoCodec is not (VideoCodec.LibaomAv1 or VideoCodec.Libvpx or VideoCodec.LibvpxVp9
                                                    or VideoCodec.Av1Nvenc or VideoCodec.Av1Amf or VideoCodec.Av1Qsv or VideoCodec.Gif),
            ContainerFormat.Mkv => videoCodec != VideoCodec.Gif,
            ContainerFormat.Webm => videoCodec is VideoCodec.LibaomAv1 or VideoCodec.Libvpx or VideoCodec.LibvpxVp9
                                               or VideoCodec.Av1Nvenc or VideoCodec.Av1Amf or VideoCodec.Av1Qsv,
            ContainerFormat.Gif => videoCodec == VideoCodec.Gif,
            _ => false
        };
    }

    public static bool IsAudioCompatible(AudioCodec audioCodec, ContainerFormat format)
    {
        if (audioCodec == AudioCodec.Copy) return true;

        return format switch
        {
            ContainerFormat.Mp4 => audioCodec is not (AudioCodec.Libspeex or AudioCodec.PcmAlaw),
            ContainerFormat.Mov => audioCodec is not (AudioCodec.Flac or AudioCodec.Libopus),
            ContainerFormat.Mkv => true,
            ContainerFormat.Webm => audioCodec is AudioCodec.Libvorbis or AudioCodec.Libopus,
            ContainerFormat.Ogg => audioCodec is AudioCodec.Flac or AudioCodec.Libvorbis or AudioCodec.Libopus or AudioCodec.Libspeex,
            ContainerFormat.Aac => audioCodec == AudioCodec.Aac,
            ContainerFormat.Wav => audioCodec is AudioCodec.Aac or AudioCodec.Flac or AudioCodec.Libvorbis
                                              or AudioCodec.PcmS16le or AudioCodec.PcmS24le or AudioCodec.PcmS32le
                                              or AudioCodec.PcmF32le or AudioCodec.PcmF64le or AudioCodec.PcmAlaw,
            ContainerFormat.Flac => audioCodec == AudioCodec.Flac,
            ContainerFormat.Mp3 => audioCodec == AudioCodec.Libmp3lame,
            ContainerFormat.Gif => false,
            _ => false
        };
    }

    public static IReadOnlyList<EncoderPreset> GetSupportedPresets(VideoCodec videoCodec)
    {
        return videoCodec switch
        {
            VideoCodec.Libx264 or VideoCodec.Libx265 => new[]
            {
                EncoderPreset.Ultrafast, EncoderPreset.Superfast, EncoderPreset.Veryfast,
                EncoderPreset.Faster, EncoderPreset.Fast, EncoderPreset.Medium,
                EncoderPreset.Slow, EncoderPreset.Slower, EncoderPreset.Veryslow, EncoderPreset.Placebo
            },
            VideoCodec.Libvpx or VideoCodec.LibvpxVp9 => new[]
            {
                EncoderPreset.Realtime, EncoderPreset.Good, EncoderPreset.Best
            },
            VideoCodec.H264Nvenc or VideoCodec.HevcNvenc or VideoCodec.Av1Nvenc => new[]
            {
                EncoderPreset.P1, EncoderPreset.P2, EncoderPreset.P3,
                EncoderPreset.P4, EncoderPreset.P5, EncoderPreset.P6, EncoderPreset.P7
            },
            VideoCodec.H264Amf or VideoCodec.HevcAmf or VideoCodec.Av1Amf => new[]
            {
                EncoderPreset.Speed, EncoderPreset.Balanced, EncoderPreset.Quality
            },
            VideoCodec.H264Qsv or VideoCodec.HevcQsv or VideoCodec.Av1Qsv => new[]
            {
                EncoderPreset.Veryfast, EncoderPreset.Faster, EncoderPreset.Fast,
                EncoderPreset.Medium, EncoderPreset.Slow, EncoderPreset.Slower, EncoderPreset.Veryslow
            },
            _ => Array.Empty<EncoderPreset>()
        };
    }

    public static IReadOnlyList<AudioCodec> GetCompatibleAudioCodecs(ContainerFormat format)
    {
        var all = (AudioCodec[])Enum.GetValues(typeof(AudioCodec));
        var list = new List<AudioCodec> { AudioCodec.Copy };
        foreach (var codec in all)
        {
            if (codec != AudioCodec.Copy && IsAudioCompatible(codec, format))
            {
                list.Add(codec);
            }
        }
        return list;
    }
}
