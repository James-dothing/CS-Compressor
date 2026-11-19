using System.IO;
using CsCompressor.Core.Models;
using CsCompressor.Core.Services;

public class FFmpegArg
{
    public enum supportedVideoEncoders
    {
        copy,
        gif,
        libx264, libx265, libaom_av1, libvpx, libvpx_vp9,
        h264_nvenc, hevc_nvenc, av1_nvenc,
        h264_amf, hevc_amf, av1_amf,
        h264_qsv, hevc_qsv, av1_qsv
    }

    public enum encodingPresets
    {
        ultrafast, superfast, veryfast, faster, fast, medium, slow, veryslow, slower, placebo,
        best, good, realtime,
        p1, p2, p3, p4, p5, p6, p7,
        speed, balanced, quality
    }

    public enum supportedAudioEncoders
    {
        copy,
        aac,
        flac,
        libmp3lame,
        libvorbis, libopus, libspeex,
        pcm_s16le, pcm_s24le, pcm_s32le, pcm_f32le, pcm_f64le, pcm_alaw,
        alac
    }

    public enum supportedOutputFormats
    {
        MP4, MOV, GIF, MKV, WebM, OGG, AAC, WAV, FLAC, MP3
    }

    public enum videoRateControlTypes
    {
        CBR, VBR, CQP, CQ
    }

    public enum scalingFlags
    {
        fast_bilinear, bilinear, bicubic, neighbor, area, bicublin, gauss, sinc, lanczos, spline
    }

    public string? ffprobePath;
    public string? inputPath;
    public string? outputPath;
    public string? startTime;
    public string? endTime;
    public bool? isTestFile;
    public string? loggingMode;
    public supportedOutputFormats? outputFormat;
    public supportedVideoEncoders? videoEncoder;
    public videoRateControlTypes? videoRateControl;
    public string? videoBitrate;
    public string? CQConstant;
    public string? QPConstant;
    public string? MinimumVideoBitrate;
    public string? MaximumVideoBitrate;
    public string? BufferSize;
    public encodingPresets? videoEncodingSpeed;
    public string? cpuUsed;
    public string[]? videoResolution;
    public scalingFlags? videoscalingflag;
    public string? videoFramerate;

    public bool? accurateColorGif;
    public bool? disableAudio;
    public supportedAudioEncoders? audioEncoder;
    public string? audioBitrate;
    public string? audioChannelAmount;
    public bool? copyMetaData;
    public bool? defaultOnError;

    public static supportedVideoEncoders? StringToVideoEncoder(string target)
    {
        var parsed = CodecHelper.ParseVideoCodec(target);
        if (parsed.HasValue && Enum.TryParse<supportedVideoEncoders>(parsed.Value.ToString().ToLowerInvariant(), true, out var result))
        {
            return result;
        }
        return Enum.TryParse<supportedVideoEncoders>(target, true, out var direct) ? direct : null;
    }

    public static supportedAudioEncoders? StringToAudioEncoder(string target)
    {
        var parsed = CodecHelper.ParseAudioCodec(target);
        if (parsed.HasValue && Enum.TryParse<supportedAudioEncoders>(parsed.Value.ToString().ToLowerInvariant(), true, out var result))
        {
            return result;
        }
        return Enum.TryParse<supportedAudioEncoders>(target, true, out var direct) ? direct : null;
    }

    public static supportedOutputFormats? StringToOutputFormat(string target)
    {
        var parsed = CodecHelper.ParseContainerFormat(target);
        if (parsed.HasValue && Enum.TryParse<supportedOutputFormats>(parsed.Value.ToString(), true, out var result))
        {
            return result;
        }
        return Enum.TryParse<supportedOutputFormats>(target, true, out var direct) ? direct : null;
    }

    public static encodingPresets? StringToEncoderPreset(string target)
    {
        return Enum.TryParse<encodingPresets>(target, true, out var result) ? result : null;
    }

    public static bool IsFormatVideo(supportedOutputFormats target)
    {
        switch (target)
        {
            case (supportedOutputFormats.OGG or supportedOutputFormats.AAC or supportedOutputFormats.WAV or supportedOutputFormats.FLAC or supportedOutputFormats.MP3):
                return false;
            default:
                return true;
        }
    }

    public static bool CheckCpuUsedCompatibility(supportedVideoEncoders target)
    {
        switch (target)
        {
            case (supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9):
                return true;
            default:
                return false;
        }
    }

    public static bool? CheckVideoCompatibility(supportedVideoEncoders videoEncoder, supportedOutputFormats outputFormat, string? ffprobePathArg = null, string? inputPathArg = null)
    {
        var format = CodecHelper.ParseContainerFormat(outputFormat.ToString());
        if (format == null) return false;

        if (videoEncoder == supportedVideoEncoders.copy)
        {
            if (string.IsNullOrEmpty(ffprobePathArg) || string.IsNullOrEmpty(inputPathArg))
            {
                return true;
            }

            string[] result = ProcessAPI.Start(ffprobePathArg, $"-v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{inputPathArg}\"");
            if (result[1] == "0" && !string.IsNullOrWhiteSpace(result[0]))
            {
                var probedCodec = CodecHelper.ParseVideoCodec(result[0].Trim());
                if (probedCodec.HasValue)
                {
                    return CodecHelper.IsVideoCompatible(probedCodec.Value, format.Value);
                }
            }
            return true;
        }

        var codec = CodecHelper.ParseVideoCodec(videoEncoder.ToString());
        return codec.HasValue && CodecHelper.IsVideoCompatible(codec.Value, format.Value);
    }

    public static bool? CheckAudioCompatibility(supportedAudioEncoders audioEncoder, supportedOutputFormats outputFormat, string? ffprobePathArg = null, string? inputPathArg = null)
    {
        var format = CodecHelper.ParseContainerFormat(outputFormat.ToString());
        if (format == null) return false;

        if (audioEncoder == supportedAudioEncoders.copy)
        {
            if (string.IsNullOrEmpty(ffprobePathArg) || string.IsNullOrEmpty(inputPathArg))
            {
                return true;
            }

            string[] result = ProcessAPI.Start(ffprobePathArg, $"-v error -select_streams a:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{inputPathArg}\"");
            if (result[1] == "0" && !string.IsNullOrWhiteSpace(result[0]))
            {
                var probedCodec = CodecHelper.ParseAudioCodec(result[0].Trim());
                if (probedCodec.HasValue)
                {
                    return CodecHelper.IsAudioCompatible(probedCodec.Value, format.Value);
                }
            }
            return true;
        }

        var codec = CodecHelper.ParseAudioCodec(audioEncoder.ToString());
        return codec.HasValue && CodecHelper.IsAudioCompatible(codec.Value, format.Value);
    }

    public static encodingPresets[] GetSupportedEncoderPresets(supportedVideoEncoders inputVideoEncoder)
    {
        var codec = CodecHelper.ParseVideoCodec(inputVideoEncoder.ToString());
        if (!codec.HasValue) return Array.Empty<encodingPresets>();

        var presets = CodecHelper.GetSupportedPresets(codec.Value);
        var result = new List<encodingPresets>();
        foreach (var p in presets)
        {
            if (Enum.TryParse<encodingPresets>(p.ToString().ToLowerInvariant(), true, out var ep))
            {
                result.Add(ep);
            }
        }
        return result.ToArray();
    }

    public static string? EncodingPresetCheck(encodingPresets inputEncodingPreset, supportedVideoEncoders inputVideoEncoder, bool crash = false)
    {
        var supported = GetSupportedEncoderPresets(inputVideoEncoder);
        if (supported.Contains(inputEncodingPreset))
        {
            return " -preset " + inputEncodingPreset.ToString();
        }
        return null;
    }

    public CompressionOptions ToCompressionOptions()
    {
        var options = new CompressionOptions
        {
            InputPath = inputPath ?? string.Empty,
            OutputPath = outputPath ?? string.Empty,
            FFprobePath = ffprobePath,
            IsTestFile = isTestFile ?? false,
            LoggingMode = loggingMode,
            EnableProgressReporting = false,
            CopyMetadata = copyMetaData ?? true
        };

        if (outputFormat.HasValue)
        {
            var fmt = CodecHelper.ParseContainerFormat(outputFormat.Value.ToString());
            if (fmt.HasValue) options.OutputFormat = fmt.Value;
        }

        if (videoEncoder.HasValue)
        {
            options.VideoCodec = CodecHelper.ParseVideoCodec(videoEncoder.Value.ToString());
        }

        if (videoRateControl.HasValue)
        {
            options.RateControl = CodecHelper.ParseRateControlMode(videoRateControl.Value.ToString());
        }

        options.VideoBitrate = videoBitrate;
        options.CQConstant = CQConstant;
        options.QPConstant = QPConstant;
        options.MinVideoBitrate = MinimumVideoBitrate;
        options.MaxVideoBitrate = MaximumVideoBitrate;
        options.BufferSize = BufferSize;

        if (videoEncodingSpeed.HasValue)
        {
            options.Preset = CodecHelper.ParsePreset(videoEncodingSpeed.Value.ToString());
        }

        if (int.TryParse(cpuUsed, out var parsedCpu))
        {
            options.CpuUsed = parsedCpu;
        }

        if (videoResolution != null && videoResolution.Length >= 2)
        {
            if (int.TryParse(videoResolution[0], out var w)) options.TargetWidth = w;
            if (int.TryParse(videoResolution[1], out var h)) options.TargetHeight = h;
        }

        if (videoscalingflag.HasValue)
        {
            options.ScalingAlgorithm = CodecHelper.ParseScalingAlgorithm(videoscalingflag.Value.ToString());
        }

        if (double.TryParse(videoFramerate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var fps))
        {
            options.Framerate = fps;
        }

        options.AccurateColorGif = accurateColorGif ?? false;
        options.DisableAudio = disableAudio ?? false;

        if (audioEncoder.HasValue)
        {
            options.AudioCodec = CodecHelper.ParseAudioCodec(audioEncoder.Value.ToString());
        }

        options.AudioBitrate = audioBitrate;

        if (int.TryParse(audioChannelAmount, out var ch))
        {
            options.AudioChannels = ch;
        }

        if (!string.IsNullOrWhiteSpace(startTime) && TimeSpan.TryParse(startTime, out var st))
        {
            options.StartTime = st;
        }

        if (!string.IsNullOrWhiteSpace(endTime) && TimeSpan.TryParse(endTime, out var et))
        {
            options.EndTime = et;
        }

        return options;
    }

    public override string? ToString()
    {
        if (string.IsNullOrEmpty(inputPath)) return null;
        if (string.IsNullOrEmpty(outputPath)) return null;
        if (!outputFormat.HasValue) return null;

        var options = ToCompressionOptions();
        var command = FFmpegArgumentBuilder.BuildCommandLine(options);

        try
        {
            if (!string.IsNullOrEmpty(ffprobePath))
            {
                var dir = Path.GetDirectoryName(ffprobePath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    File.WriteAllText(Path.Combine(dir, "LastFFmpegArg.txt"), command);
                }
            }
        }
        catch
        {
            // Ignore debug logging side-effect exceptions
        }

        return command;
    }
}
