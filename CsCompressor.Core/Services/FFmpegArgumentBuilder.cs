using System.Globalization;
using System.Text;
using CsCompressor.Core.Models;

namespace CsCompressor.Core.Services;

public static class FFmpegArgumentBuilder
{
    public static IReadOnlyList<string> BuildArgumentList(CompressionOptions options)
    {
        var args = new List<string> { "-y" };

        if (options.EnableProgressReporting)
        {
            args.Add("-progress");
            args.Add("pipe:1");
            args.Add("-nostats");
        }

        if (!string.IsNullOrWhiteSpace(options.LoggingMode))
        {
            args.Add("-v");
            args.Add(options.LoggingMode);
        }

        if (options.StartTime.HasValue && options.StartTime.Value > TimeSpan.Zero)
        {
            args.Add("-ss");
            args.Add(FormatTimeSpan(options.StartTime.Value));
        }

        if (options.EndTime.HasValue && options.EndTime.Value > TimeSpan.Zero)
        {
            args.Add("-to");
            args.Add(FormatTimeSpan(options.EndTime.Value));
        }

        if (options.IsTestFile)
        {
            args.Add("-f");
            args.Add("lavfi");
        }

        args.Add("-i");
        args.Add(options.InputPath);

        // Determine if video filters are required
        bool hasVideoFilters = (options.TargetWidth.HasValue && options.TargetHeight.HasValue)
                               || (options.Framerate.HasValue && options.Framerate.Value > 0)
                               || (options.AccurateColorGif && options.OutputFormat == ContainerFormat.Gif);

        bool isAudioOnly = CodecHelper.IsAudioOnlyFormat(options.OutputFormat);

        if (!isAudioOnly)
        {
            if (hasVideoFilters)
            {
                var filterParts = new List<string>();

                if (options.Framerate.HasValue && options.Framerate.Value > 0)
                {
                    filterParts.Add($"fps={options.Framerate.Value.ToString(CultureInfo.InvariantCulture)}");
                }

                if (options.TargetWidth.HasValue && options.TargetHeight.HasValue)
                {
                    string scaleStr = $"scale={options.TargetWidth.Value}:{options.TargetHeight.Value}";
                    if (options.ScalingAlgorithm.HasValue)
                    {
                        scaleStr += $":flags={CodecHelper.ToScalingAlgorithmName(options.ScalingAlgorithm.Value)}";
                    }
                    filterParts.Add(scaleStr);
                }

                if (options.AccurateColorGif && options.OutputFormat == ContainerFormat.Gif)
                {
                    filterParts.Add("split[a][b];[a]palettegen[p];[b][p]paletteuse=dither=sierra2_4a");
                }

                string filterGraph = $"[0:v]{string.Join(",", filterParts)}[outv]";
                args.Add("-filter_complex");
                args.Add(filterGraph);
                args.Add("-map");
                args.Add("[outv]");
            }
            else
            {
                // No filters: map video stream directly
                args.Add("-map");
                args.Add("0:v:0?");
            }

            // Video codec and rate control
            if (options.VideoCodec.HasValue)
            {
                args.Add("-c:v");
                args.Add(CodecHelper.ToFFmpegVideoCodecName(options.VideoCodec.Value));

                ApplyVideoEncodingSettings(args, options);
            }
        }

        // Audio stream mapping and encoding
        if (options.DisableAudio || options.OutputFormat == ContainerFormat.Gif)
        {
            args.Add("-an");
        }
        else
        {
            args.Add("-map");
            args.Add("0:a:0?");

            if (options.AudioCodec.HasValue)
            {
                args.Add("-c:a");
                args.Add(CodecHelper.ToFFmpegAudioCodecName(options.AudioCodec.Value));

                if (!string.IsNullOrWhiteSpace(options.AudioBitrate))
                {
                    args.Add("-b:a");
                    args.Add(options.AudioBitrate);
                }

                if (options.AudioChannels.HasValue && options.AudioChannels.Value > 0)
                {
                    args.Add("-ac");
                    args.Add(options.AudioChannels.Value.ToString());
                }
            }
        }

        // Metadata preservation
        args.Add("-map_metadata");
        args.Add(options.CopyMetadata ? "0" : "-1");

        // Container format
        args.Add("-f");
        args.Add(CodecHelper.ToFFmpegFormatName(options.OutputFormat));

        // Destination path
        args.Add(options.OutputPath);

        return args;
    }

    private static void ApplyVideoEncodingSettings(List<string> args, CompressionOptions options)
    {
        var codec = options.VideoCodec!.Value;

        if (options.RateControl.HasValue)
        {
            switch (options.RateControl.Value)
            {
                case RateControlMode.Cbr:
                    if (!string.IsNullOrWhiteSpace(options.VideoBitrate))
                    {
                        if (codec is VideoCodec.H264Nvenc or VideoCodec.HevcNvenc or VideoCodec.Av1Nvenc)
                        {
                            args.Add("-rc");
                            args.Add("cbr");
                            args.Add("-b:v");
                            args.Add(options.VideoBitrate);
                        }
                        else if (codec is VideoCodec.H264Amf or VideoCodec.HevcAmf or VideoCodec.Av1Amf)
                        {
                            args.Add("-rc");
                            args.Add("cbr");
                            args.Add("-b:v");
                            args.Add(options.VideoBitrate);
                        }
                        else
                        {
                            args.Add("-b:v");
                            args.Add(options.VideoBitrate);
                            args.Add("-minrate");
                            args.Add(options.VideoBitrate);
                            args.Add("-maxrate");
                            args.Add(options.VideoBitrate);
                            if (!string.IsNullOrWhiteSpace(options.BufferSize))
                            {
                                args.Add("-bufsize");
                                args.Add(options.BufferSize);
                            }
                        }
                    }
                    break;

                case RateControlMode.Vbr:
                    if (!string.IsNullOrWhiteSpace(options.VideoBitrate))
                    {
                        if (codec is VideoCodec.H264Nvenc or VideoCodec.HevcNvenc or VideoCodec.Av1Nvenc)
                        {
                            args.Add("-rc");
                            args.Add("vbr");
                        }
                        else if (codec is VideoCodec.H264Amf or VideoCodec.HevcAmf or VideoCodec.Av1Amf)
                        {
                            args.Add("-rc");
                            args.Add("vbr_peak");
                        }

                        args.Add("-b:v");
                        args.Add(options.VideoBitrate);

                        if (!string.IsNullOrWhiteSpace(options.MinVideoBitrate))
                        {
                            args.Add("-minrate");
                            args.Add(options.MinVideoBitrate);
                        }

                        if (!string.IsNullOrWhiteSpace(options.MaxVideoBitrate))
                        {
                            args.Add("-maxrate");
                            args.Add(options.MaxVideoBitrate);
                        }
                    }
                    break;

                case RateControlMode.Cqp:
                    if (!string.IsNullOrWhiteSpace(options.QPConstant))
                    {
                        if (codec is VideoCodec.H264Nvenc or VideoCodec.HevcNvenc or VideoCodec.Av1Nvenc)
                        {
                            args.Add("-rc");
                            args.Add("constqp");
                            args.Add("-qp");
                            args.Add(options.QPConstant);
                        }
                        else if (codec is VideoCodec.H264Amf or VideoCodec.HevcAmf or VideoCodec.Av1Amf)
                        {
                            args.Add("-rc");
                            args.Add("cqp");
                            args.Add("-qp_i");
                            args.Add(options.QPConstant);
                            args.Add("-qp_p");
                            args.Add(options.QPConstant);
                        }
                        else if (codec is VideoCodec.H264Qsv or VideoCodec.HevcQsv or VideoCodec.Av1Qsv)
                        {
                            args.Add("-qscale:v");
                            args.Add("1");
                            args.Add("-global_quality");
                            args.Add(options.QPConstant);
                        }
                        else
                        {
                            args.Add("-qp");
                            args.Add(options.QPConstant);
                        }
                    }
                    break;

                case RateControlMode.Cq:
                    if (!string.IsNullOrWhiteSpace(options.CQConstant))
                    {
                        if (codec is VideoCodec.H264Nvenc or VideoCodec.HevcNvenc or VideoCodec.Av1Nvenc)
                        {
                            args.Add("-rc");
                            args.Add("vbr");
                            args.Add("-cq");
                            args.Add(options.CQConstant);
                            args.Add("-b:v");
                            args.Add("0");
                        }
                        else if (codec is VideoCodec.H264Amf or VideoCodec.HevcAmf or VideoCodec.Av1Amf)
                        {
                            args.Add("-rc");
                            args.Add("qvbr");
                            args.Add("-qvbr_quality_level");
                            args.Add(options.CQConstant);
                        }
                        else if (codec is VideoCodec.H264Qsv or VideoCodec.HevcQsv or VideoCodec.Av1Qsv)
                        {
                            args.Add("-global_quality");
                            args.Add(options.CQConstant);
                        }
                        else if (codec is VideoCodec.LibaomAv1 or VideoCodec.Libvpx or VideoCodec.LibvpxVp9)
                        {
                            args.Add("-crf");
                            args.Add(options.CQConstant);
                            args.Add("-b:v");
                            args.Add("0");
                        }
                        else
                        {
                            args.Add("-crf");
                            args.Add(options.CQConstant);
                        }
                    }
                    break;
            }
        }

        // Encoder preset
        if (options.Preset.HasValue)
        {
            var presetStr = options.Preset.Value.ToString().ToLowerInvariant();
            if (codec is VideoCodec.Libvpx or VideoCodec.LibvpxVp9)
            {
                args.Add("-deadline");
                args.Add(presetStr);
            }
            else
            {
                args.Add("-preset");
                args.Add(presetStr);
            }
        }

        // CPU Used for VP8 / VP9 / AV1
        if (options.CpuUsed.HasValue && codec is VideoCodec.LibaomAv1 or VideoCodec.Libvpx or VideoCodec.LibvpxVp9)
        {
            args.Add("-cpu-used");
            args.Add(options.CpuUsed.Value.ToString());
        }
    }

    public static string EscapeArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
            return "\"\"";

        bool needsQuotes = argument.Any(c => char.IsWhiteSpace(c) || c == '"' || c == '$' || c == '&' || c == ';');

        if (!needsQuotes)
            return argument;

        // Escape double quotes as \"
        var escaped = argument.Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    public static string BuildCommandLine(CompressionOptions options)
    {
        var argList = BuildArgumentList(options);
        var sb = new StringBuilder();

        for (int i = 0; i < argList.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(EscapeArgument(argList[i]));
        }

        return sb.ToString();
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}",
            (int)ts.TotalHours, ts.Minutes, ts.Seconds, ts.Milliseconds);
    }
}
