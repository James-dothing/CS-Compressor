using System.Text.RegularExpressions;
using CsCompressor.Core.Models;
using CsCompressor.Core.Services;

namespace CsCompressor.Core.Validation;

public static class CompressionOptionsValidator
{
    private static readonly Regex BitrateRegex = new(@"^\d+(?:\.\d+)?\s*(?:k|m|g|kbps|mbps)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ValidationResult Validate(CompressionOptions options)
    {
        var result = new ValidationResult();

        // 1. Input Path Validation
        if (string.IsNullOrWhiteSpace(options.InputPath))
        {
            result.AddError(nameof(options.InputPath), "Input file path is required.");
        }
        else if (!options.IsTestFile)
        {
            if (!File.Exists(options.InputPath))
            {
                result.AddError(nameof(options.InputPath), $"Input file does not exist: {options.InputPath}");
            }
            else
            {
                try
                {
                    var fileInfo = new FileInfo(options.InputPath);
                    if (fileInfo.Length == 0)
                    {
                        result.AddError(nameof(options.InputPath), "Input file is empty (0 bytes).");
                    }
                }
                catch (Exception ex)
                {
                    result.AddError(nameof(options.InputPath), $"Cannot access input file: {ex.Message}");
                }
            }
        }

        // 2. Output Path Validation
        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            result.AddError(nameof(options.OutputPath), "Output file path is required.");
        }
        else
        {
            try
            {
                var outputDir = Path.GetDirectoryName(options.OutputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    // Check if parent exists or can be created
                    var root = Path.GetPathRoot(options.OutputPath);
                    if (!string.IsNullOrEmpty(root) && !Directory.Exists(root))
                    {
                        result.AddError(nameof(options.OutputPath), $"Output root drive does not exist: {root}");
                    }
                }

                if (File.Exists(options.OutputPath))
                {
                    result.AddWarning(nameof(options.OutputPath), $"Output file already exists and will be overwritten: {Path.GetFileName(options.OutputPath)}");
                }
            }
            catch (Exception ex)
            {
                result.AddError(nameof(options.OutputPath), $"Invalid output path: {ex.Message}");
            }
        }

        // Check if input and output are the same
        if (!string.IsNullOrWhiteSpace(options.InputPath) && !string.IsNullOrWhiteSpace(options.OutputPath))
        {
            try
            {
                if (string.Equals(Path.GetFullPath(options.InputPath), Path.GetFullPath(options.OutputPath), StringComparison.OrdinalIgnoreCase))
                {
                    result.AddError(nameof(options.OutputPath), "Input and output files cannot be the same file.");
                }
            }
            catch
            {
                // Ignore path resolution errors
            }
        }

        // 3. Codec Compatibility Validation
        if (!CodecHelper.IsAudioOnlyFormat(options.OutputFormat))
        {
            if (options.VideoCodec.HasValue && !CodecHelper.IsVideoCompatible(options.VideoCodec.Value, options.OutputFormat))
            {
                result.AddError(nameof(options.VideoCodec),
                    $"Video encoder '{options.VideoCodec.Value}' is not supported by container format '{options.OutputFormat}'.");
            }
        }

        if (!options.DisableAudio && options.AudioCodec.HasValue)
        {
            if (!CodecHelper.IsAudioCompatible(options.AudioCodec.Value, options.OutputFormat))
            {
                result.AddError(nameof(options.AudioCodec),
                    $"Audio encoder '{options.AudioCodec.Value}' is not supported by container format '{options.OutputFormat}'.");
            }
        }

        // 4. Rate Control Validation
        if (options.RateControl.HasValue)
        {
            switch (options.RateControl.Value)
            {
                case RateControlMode.Cbr or RateControlMode.Vbr:
                    if (string.IsNullOrWhiteSpace(options.VideoBitrate))
                    {
                        result.AddError(nameof(options.VideoBitrate), "Video bitrate is required for CBR/VBR rate control (e.g. '2M' or '2500k').");
                    }
                    else if (!BitrateRegex.IsMatch(options.VideoBitrate.Trim()))
                    {
                        result.AddError(nameof(options.VideoBitrate), $"Invalid video bitrate format: '{options.VideoBitrate}'. Expected number with optional 'k' or 'M' suffix (e.g. '1500k', '4M').");
                    }
                    break;

                case RateControlMode.Cqp:
                    if (string.IsNullOrWhiteSpace(options.QPConstant))
                    {
                        result.AddError(nameof(options.QPConstant), "QP constant value is required for CQP rate control.");
                    }
                    else if (!int.TryParse(options.QPConstant.Trim(), out var qp) || qp < 0 || qp > 63)
                    {
                        result.AddError(nameof(options.QPConstant), $"QP constant must be an integer between 0 and 63 (got '{options.QPConstant}').");
                    }
                    break;

                case RateControlMode.Cq:
                    if (string.IsNullOrWhiteSpace(options.CQConstant))
                    {
                        result.AddError(nameof(options.CQConstant), "CQ/CRF constant value is required for CQ rate control.");
                    }
                    else if (!double.TryParse(options.CQConstant.Trim(), out var cq) || cq < 0 || cq > 63)
                    {
                        result.AddError(nameof(options.CQConstant), $"CQ/CRF constant must be a number between 0 and 63 (got '{options.CQConstant}').");
                    }
                    break;
            }
        }

        // 5. Resolution Validation
        if (options.TargetWidth.HasValue || options.TargetHeight.HasValue)
        {
            if (!options.TargetWidth.HasValue || !options.TargetHeight.HasValue)
            {
                result.AddError(nameof(options.TargetWidth), "Both width and height must be specified for resolution scaling.");
            }
            else
            {
                if (options.TargetWidth.Value <= 0 || options.TargetHeight.Value <= 0)
                {
                    result.AddError(nameof(options.TargetWidth), "Width and height must be positive numbers.");
                }
                else
                {
                    if (options.TargetWidth.Value % 2 != 0 || options.TargetHeight.Value % 2 != 0)
                    {
                        result.AddWarning(nameof(options.TargetWidth),
                            $"Resolution {options.TargetWidth}×{options.TargetHeight} contains odd dimensions. Many H.264/H.265 encoders require even dimensions.");
                    }
                }
            }
        }

        // 6. Framerate Validation
        if (options.Framerate.HasValue && options.Framerate.Value <= 0)
        {
            result.AddError(nameof(options.Framerate), "Framerate must be a positive number.");
        }

        // 7. Trimming Validation
        if (options.StartTime.HasValue && options.EndTime.HasValue)
        {
            if (options.StartTime.Value >= options.EndTime.Value)
            {
                result.AddError(nameof(options.StartTime),
                    $"Start time ({options.StartTime.Value}) must be before end time ({options.EndTime.Value}).");
            }
        }

        return result;
    }
}
